using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallBlock : MonoBehaviour
{
    [SerializeField] private GameObject destroyedFx;
    private MeshRenderer rend;
    private Grid3D grid;

    private float healthMax;
    private float healthCurr;

    void Start()
    {
        grid = GameObject.FindGameObjectWithTag("Grid").GetComponent<Grid3D>();
        rend = GetComponent<MeshRenderer>();

        healthMax = 500;
        healthCurr = healthMax;
    }

    public void Damaged(float dmg)
    {
        if (healthCurr <= 0)
        {
            return;
        }

        if (healthCurr - dmg <= 0)  // Will be destroyed
        {
            healthCurr -= dmg;
            StartCoroutine(FlashBreak());
        }
        else // Damaged, will not be destroyed
        {
            healthCurr -= dmg;

            float blend1;
            float blend2;
            float blend3;
            float missingHealth = healthMax - healthCurr;

            if (missingHealth <= 150) // Blend texture 1 based on missing health
            {
                blend1 = (healthMax - healthCurr) / 30;
                blend2 = 0;
                blend3 = 0;
            }
            else if (missingHealth <= 300) // Blend texture 2 based on missing health, cap texture 1 to 5
            {
                blend1 = 5;
                blend2 = ((healthMax - 150) - healthCurr) / 30;
                blend3 = 0;
            }
            else // Blend texture 3 based on missing health, cap texture 2 to 5, decrease texture 1 blend based on current texture 3 blend
            {
                blend2 = 5;
                blend3 = ((healthMax - 300) - healthCurr) / 30;

                blend1 = 5 - blend3;
            }

            rend.material.SetFloat("_Blend1", blend1);
            rend.material.SetFloat("_Blend2", blend2);
            rend.material.SetFloat("_Blend3", blend3);
        }
    }

    private IEnumerator FlashBreak()
    {
        float waitTime;
        float elapsedTime;

        float startBlend1 = rend.material.GetFloat("_Blend1");
        float startBlend2 = rend.material.GetFloat("_Blend2");
        float startBlend3 = rend.material.GetFloat("_Blend3");

        waitTime = .1f;
        elapsedTime = 0f;
        while (elapsedTime < waitTime)
        {
            float blend1 = Mathf.Lerp(startBlend1, 0, elapsedTime / waitTime);
            float blend2 = Mathf.Lerp(startBlend2, 40, elapsedTime / waitTime);
            float blend3 = Mathf.Lerp(startBlend3, 40, elapsedTime / waitTime);

            rend.material.SetFloat("_Blend1", blend1);
            rend.material.SetFloat("_Blend2", blend2);
            rend.material.SetFloat("_Blend3", blend3);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        waitTime = .05f;
        elapsedTime = 0;
        while (elapsedTime < waitTime)
        {
            float blend1 = Mathf.Lerp(startBlend1, 0, elapsedTime / waitTime);
            float blend2 = Mathf.Lerp(startBlend2, 5, elapsedTime / waitTime);
            float blend3 = Mathf.Lerp(startBlend3, 5, elapsedTime / waitTime);

            rend.material.SetFloat("_Blend1", blend1);
            rend.material.SetFloat("_Blend2", blend2);
            rend.material.SetFloat("_Blend3", blend3);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        // Create destroyed effect
        GameObject obj = Instantiate(destroyedFx, transform.position, Quaternion.identity);
        obj.SetActive(true);

        // Update the grid node isWalkable at this wall block's position
        gameObject.SetActive(false); // Set inactive before updating so grid won't see the block
        grid.UpdateNodeWalkable(transform.position);

        // Destroy this wall block
        Destroy(gameObject);
    }

}
