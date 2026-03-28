using UnityEngine;

public class CatManager : MonoBehaviour
{
    [SerializeField] private CatController _catLeftController;
    [SerializeField] private CatController _catRightController;

    public void PlayLoseAnimation(Tile failedTile)
    {
        if (_catLeftController != null)
        {
            _catLeftController.PlayLose();
        }

        if (_catRightController != null)
        {
            _catRightController.PlayLose();
        }
    }

    public void PlayWinAnimation()
    {
        if (_catLeftController != null)
        {
            _catLeftController.PlayWin();
        }

        if (_catRightController != null)
        {
            _catRightController.PlayWin();
        }
    }


    public bool TryEatTile(Tile tile, float hitToleranceX)
    {
        int pid = tile.Data.pid;
        CatController targetCat = (pid == 0 || pid == 2) ? _catLeftController : _catRightController;

        float catX = targetCat.transform.position.x;
        float tileX = tile.transform.position.x;

        if (Mathf.Abs(catX - tileX) <= hitToleranceX)
        {
            float ts = tile.Data.ts;

            if (ts > 0.01f)
            {
                targetCat.PlayEatLong(ts);
            }
            else
            {
                targetCat.PlayEatSingle();
            }

            return true; 
        }

        return false; 
    }
    
}