using Unity.Burst;
using Unity.Collections;
using UnityEngine.Jobs;

/// <summary>
/// Job for moving belt objects of an asteroid belt.
/// BurstCompile marks this job for the BurstCompiler / Job System.
/// </summary>
/// <Steps>
/// 1. Take the belt object data
/// 2. Update the _transform parameter by using the BeltObject.Data Update method.
/// </Steps>
[BurstCompile]
public struct BeltObjectMovementJob : IJobParallelForTransform
{
    /// <summary>
    /// Collection of data of all BeltObjects that are worked on inside this thread. Specific data is accessed through the index parameter of the Execute method
    /// </summary>
    [ReadOnly]
    public NativeArray<BeltObject.Data> dataArray;

    /// <summary>
    /// Time.deltaTime passed in through the main thread. Is read-only, because it does't change in the job
    /// </summary>
    [ReadOnly]
    public float deltaTime;

    /// <summary>
    /// Executes the job and updates the position / rotation of _transform
    /// </summary>
    /// <param name="index">Index of the current TransformAccess parameter of the list handled by the job</param>
    /// <param name="_transform">Current TransformAccess being updated</param>
    public void Execute(int index, TransformAccess _transform)
    {
        var data = dataArray[index];
        data.UpdateTransformAccess(_transform, deltaTime);
    }
}