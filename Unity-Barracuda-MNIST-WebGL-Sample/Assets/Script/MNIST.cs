using Unity.Barracuda;

public class MNIST
{   
    readonly IWorker worker;

    public MNIST(NNModel modelAsset)
    {
        var model = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.CSharpBurst, model);
    }

    public float[] Inference(float[] inputFloats)
    {
        var inputTensor = new Tensor(1, 28, 28, 1, inputFloats);

        worker.Execute(inputTensor);
        var outputTensor = worker.PeekOutput();
        var outputArray = outputTensor.ToReadOnlyArray();
        
        inputTensor.Dispose();
        outputTensor.Dispose();

        return outputArray;
    }

    ~MNIST()
    {
        worker?.Dispose();
    }
}