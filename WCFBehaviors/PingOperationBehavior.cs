using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace WCFBehaviors {
    public class PingOperationBehavior : IOperationBehavior {
        /// <summary>
        /// Modification or extension of the service across an operation. Adds the Ping operation.
        /// </summary>
        /// <param name="operationDescription"></param>
        /// <param name="dispatchOperation"></param>
        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation) {
            dispatchOperation.Invoker = new PingInvoker();
        }

        /// <summary>
        /// Confirm that the operation meets some intended criteria
        /// </summary>
        /// <param name="operationDescription"></param>
        public void Validate(OperationDescription operationDescription) { }
        /// <summary>
        /// Pass data at runtime to bindings to support custom behavior.
        /// </summary>
        /// <param name="operationDescription"></param>
        /// <param name="bindingParameters"></param>
        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters) { }
        /// <summary>
        /// Modification or extension of the client across an operation.
        /// </summary>
        /// <param name="operationDescription"></param>
        /// <param name="clientOperation"></param>
        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation) { }
    }

    internal class PingInvoker : IOperationInvoker {
        public object[] AllocateInputs() {
            return new object[0];
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs) {
            outputs = new object[0];
            return DateTime.UtcNow;
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state) {
            throw new NotImplementedException();
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result) {
            throw new NotImplementedException();
        }

        public bool IsSynchronous {
            get { return true; }
        }
    }
}

