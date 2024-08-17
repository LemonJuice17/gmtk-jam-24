using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssetRandomizer : MonoBehaviour
{
    //[SerializeField] Transform targetTransform;

    [SerializeField][Range(0, 2)] float positionVarianceX;
    [SerializeField][Range(0, 2)] float positionVarianceY;
    [SerializeField][Range(0, 2)] float positionVarianceZ;
    [SerializeField][Range(0, 180)] float rotationVarianceX;
    [SerializeField][Range(0, 180)] float rotationVarianceY;
    [SerializeField][Range(0, 180)] float rotationVarianceZ;
    [SerializeField][Range(0, 2)] float scaleVarianceX;
    [SerializeField][Range(0, 2)] float scaleVarianceY;
    [SerializeField][Range(0, 2)] float scaleVarianceZ;

    private void Awake()
    {
        Vector3 targetPosition = new Vector3(Random.Range(-positionVarianceX, positionVarianceX),
                                             Random.Range(-positionVarianceY, positionVarianceY),
                                             Random.Range(-positionVarianceZ, positionVarianceZ));
        Vector3 targetRotation = new Vector3(Random.Range(-rotationVarianceX, rotationVarianceX),
                                             Random.Range(-rotationVarianceY, rotationVarianceY),
                                             Random.Range(-rotationVarianceZ, rotationVarianceZ));
        Vector3 targetScale = new Vector3(Random.Range(-scaleVarianceX, scaleVarianceX),
                                             Random.Range(-scaleVarianceY, scaleVarianceY),
                                             Random.Range(-scaleVarianceZ, scaleVarianceZ));

        transform.position = transform.position + targetPosition;
        transform.rotation = Quaternion.Euler(targetRotation.x, targetRotation.y, targetRotation.z);
        transform.localScale = transform.localScale + targetScale;
    }
}
