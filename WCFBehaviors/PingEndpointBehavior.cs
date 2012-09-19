using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace WCFBehaviors {
    /// <summary>
    /// WCF Behavior that adds an ExpertPing method to an endpoint
    /// </summary>
    /// <remarks>ExpertPing is used for the operation name to avoid clashes with previous ping implementations.</remarks>
    public class PingEndpointBehavior : BehaviorExtensionElement, IEndpointBehavior {
        private const string PingOperationName = "Ping";
        private const string PingResponse = "PingResponse";

        #region IEndpointBehavior
        public void Validate(ServiceEndpoint endpoint) {}
        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters) {}
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime) {}

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher) {
            if(PingOperationNotDeclaredInContract(endpoint.Contract)) {
                AddPingToContractDescription(endpoint.Contract);
            }

            UpdateContractFilter(endpointDispatcher, endpoint.Contract);
            AddPingToDispatcher(endpointDispatcher, endpoint.Contract);
        }

        private bool PingOperationNotDeclaredInContract(ContractDescription contract) {
            return ! contract
                .Operations
                .Where(operationDescription => operationDescription.Name.Equals(PingOperationName, StringComparison.InvariantCultureIgnoreCase))
                .Any();
        }

        /// <summary>
        /// Add the Ping method to the existing contract
        /// </summary>
        private void AddPingToContractDescription(ContractDescription contractDescription) {
            OperationDescription pingOperationDescription = new OperationDescription(PingOperationName, contractDescription);

            MessageDescription inputMessageDescription = new MessageDescription(
                GetAction(contractDescription, PingOperationName),
                MessageDirection.Input);

            MessageDescription outputMessageDescription = new MessageDescription(
                GetAction(contractDescription, PingResponse),
                MessageDirection.Output);

            MessagePartDescription returnValue = new MessagePartDescription("PingResult", contractDescription.Namespace);

            returnValue.Type = typeof(DateTime);
            outputMessageDescription.Body.ReturnValue = returnValue;

            inputMessageDescription.Body.WrapperName = PingOperationName;
            inputMessageDescription.Body.WrapperNamespace = contractDescription.Namespace;
            outputMessageDescription.Body.WrapperName = PingResponse;
            outputMessageDescription.Body.WrapperNamespace = contractDescription.Namespace;

            pingOperationDescription.Messages.Add(inputMessageDescription);
            pingOperationDescription.Messages.Add(outputMessageDescription);

            pingOperationDescription.Behaviors.Add(new DataContractSerializerOperationBehavior(pingOperationDescription));
            pingOperationDescription.Behaviors.Add(new PingOperationBehavior());

            contractDescription.Operations.Add(pingOperationDescription);
        }

        /// <summary>
        /// Sets the contract filter to support all operations currently registered in the contract description
        /// </summary>
        /// <param name="endpointDispatcher">Endpoint dispatcher responsible for issuing operations</param>
        /// <param name="contractDescription">Contract the ping operation is being added to</param>
        private void UpdateContractFilter(EndpointDispatcher endpointDispatcher, ContractDescription contractDescription) {
            string[] actions = (from 
                                    operationDescription in contractDescription.Operations
                                select
                                    GetAction(contractDescription, operationDescription.Name)
                               ).ToArray();

            endpointDispatcher.ContractFilter = new ActionMessageFilter(actions);
        }
        
        /// <summary>
        /// Add the Ping dispatcher operation to the DispatchRuntime
        /// </summary>
        /// <param name="endpointDispatcher">Endpoint dispatcher responsible for issuing operations</param>
        /// <param name="contractDescription">Contract the ping operation is being added to</param>
        private void AddPingToDispatcher(EndpointDispatcher endpointDispatcher, ContractDescription contractDescription) {
            DispatchOperation pingDispatchOperation = new DispatchOperation(endpointDispatcher.DispatchRuntime,
                                                                            PingOperationName,
                                                                            GetAction(contractDescription, PingOperationName),
                                                                            GetAction(contractDescription, PingResponse));
            pingDispatchOperation.Invoker = new PingInvoker();
            endpointDispatcher.DispatchRuntime.Operations.Add(pingDispatchOperation);
        }        

        private string GetAction(ContractDescription contractDescription, string name) {
            string @namespace = contractDescription.Namespace;

            if(!@namespace.EndsWith("/")) { @namespace = @namespace + "/"; }
            
            string action = string.Format("{0}{1}/{2}", @namespace, contractDescription.Name, name);
            Trace.WriteLine(string.Format("Action '{0}'", action));
            return action;
        }

        #endregion
        
        #region BehaviorExtensionElement
        protected override object CreateBehavior() {
            return new PingEndpointBehavior();
        }

        public override Type BehaviorType {
            get { return typeof (PingEndpointBehavior); }
        }
        #endregion
    }
}
