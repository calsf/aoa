using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallBlockDestroyed : MonoBehaviour
{
    public void OnEnable()
    {
        // Apply explosion on all child blocks
        foreach (Transform child in transform)
        {
            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.constraints = RigidbodyConstraints.None;
                rb.AddExplosionForce(200, transform.position, 0, .02f);

                StartCoroutine(Fade(child.gameObject));
            }
        }
    }

    // Fade out a child block after some time
    IEnumerator Fade(GameObject obj)
    {
        yield return new WaitForSeconds(2f);

        Material material = obj.GetComponent<MeshRenderer>().material;
        float waitTime = .8f;
        float elapsedTime = 0f;
        float alpha = material.GetFloat("_Alpha");
        while (elapsedTime < waitTime)
        {
            float newAlpha = Mathf.Lerp(alpha, .4f, elapsedTime / waitTime);

            material.SetFloat("_Alpha", newAlpha);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // All blocks should have gone through Fade, destroy entire game object
        Destroy(gameObject);
    }
}
