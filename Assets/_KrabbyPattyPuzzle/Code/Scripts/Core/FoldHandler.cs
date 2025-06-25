using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace YRA
{
    public class FoldHandler : IFoldable
    {
        private FoldingBlock block;
        private FoldSettings settings;
        public UnityAction onFoldedEvent;

        public FoldHandler(FoldingBlock block, FoldSettings settings)
        {
            this.block = block;
            this.settings = settings;
        }

        public bool CanFoldInDirection(FoldDirection direction)
        {
            if (block == null)
                Debug.Log("BLOCK IS NULL");
            if (!block.FoldPermissions.CanFoldInDirection(direction))
                return false;

            return GetAdjacentBlock(direction) != null;
        }

        public void ExecuteFold(FoldDirection direction, float dragStrength)
        {
            if (block.State.IsFolding || !CanFoldInDirection(direction))
                return;
            FoldingBlock foldTarget = GetAdjacentBlock(direction);
            block.StartCoroutine(PerformFold(direction, dragStrength, foldTarget));
        }

        public FoldDirection GetDirection(Vector2 dragVector)
        {
            throw new System.NotImplementedException();
        }

        public FoldingBlock GetAdjacentBlock(FoldDirection direction)
        {
            Vector3 checkDirection = GetWorldDirectionFromFold(direction);
            RaycastHit hit;
            if (Physics.Raycast(block.transform.position, checkDirection, out hit, 1f))
            {
                return hit.collider.GetComponent<FoldingBlock>();
            }
            return null;
        }

        public void HidePreview(FoldingBlock block)
        {
            throw new System.NotImplementedException();
        }

        void CompleteFold(bool _isFolded, FoldingBlock target)
        {
            if (_isFolded)
            {
                Transform parent = target.transform;
                block.transform.SetParent(parent, true);
                block.GetComponent<BoxCollider>().enabled = false;
                GameManager.Instance.AddFoldedCollection(block);
                block.State.IsSelectable = false;
                block.State.IsFolded = true;
            }
            else
            {
                block.transform.SetParent(null, true);
                block.GetComponent<BoxCollider>().enabled = true;
                GameManager.Instance.RemoveFoldedCollection(block);
                block.State.IsSelectable = true;
                block.State.IsFolded = false;
            }

            AudioHandler.Instance.PlaySizzle();
            onFoldedEvent?.Invoke();
        }

        public void InvalidFold()
        {
            block.StartCoroutine(ShakeBlock(block));
        }

        public void ShowPreview(FoldingBlock block, FoldDirection direction, float strength)
        {
            throw new System.NotImplementedException();
        }

        private IEnumerator PerformFold(FoldDirection direction, float dragStrength, FoldingBlock target)
        {
            yield return new WaitForEndOfFrame();
            block.State.IsFolding = true;
            block.State.IsSelectable = false;
            float targetAngle = Mathf.Lerp(0f, settings.snapAngle, dragStrength);
            bool shouldSnap = targetAngle >= settings.snapAngle * 0.8f;
            if (shouldSnap)
                targetAngle = 179f;
            Vector3 offset = GetFoldOffset(direction);
            int stacks = GetStackCount(target);
            Vector3 endPos = block.transform.position + offset + (target.transform.up * .07f * stacks);

            yield return AnimateFold(direction, targetAngle, target, endPos);
            
            CompleteFold(true, target);
            block.State.IsFolding = false;
        }

        private IEnumerator AnimateFold(FoldDirection direction, float targetAngle, FoldingBlock foldedBlock, Vector3 endPos)
        {
            float elapsedTime = 0f;
            float duration = 1f / settings.foldSpeed;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / duration;
                float curveValue = settings.foldCurve.Evaluate(progress);
                float currentFoldAngle = Mathf.Lerp(0f, targetAngle, curveValue);
                Vector3 rotation = block.originalRotation;

                if (direction == FoldDirection.Up || direction == FoldDirection.Down)
                {
                    rotation.x += currentFoldAngle * (direction == FoldDirection.Up ? 1 : -1);
                }
                else if (direction == FoldDirection.Left || direction == FoldDirection.Right)
                {
                    rotation.z += currentFoldAngle * (direction == FoldDirection.Left ? 1 : -1);
                }

                block.transform.eulerAngles = rotation;
                block.transform.position = Vector3.Lerp(block.transform.position, endPos, progress);
                yield return null;
            }
        }

        private int GetStackCount(FoldingBlock foldTarget)
        {
            FoldingBlock[] targetChild = foldTarget.transform.GetComponentsInChildren<FoldingBlock>();
            FoldingBlock[] thisChild = block.transform.GetComponentsInChildren<FoldingBlock>();
            return targetChild.Length + thisChild.Length;
        }

        private Vector3 GetFoldOffset(FoldDirection direction)
        {
            switch (direction)
            {
                case FoldDirection.Up:
                    return Vector3.forward * settings.blockSize;
                case FoldDirection.Down:
                    return Vector3.back * settings.blockSize;
                case FoldDirection.Left:
                    return Vector3.left * settings.blockSize;
                case FoldDirection.Right:
                    return Vector3.right * settings.blockSize;
                default:
                    return Vector3.zero;
            }
        }

        private Vector3 GetWorldDirectionFromFold(FoldDirection direction)
        {
            switch (direction)
            {
                case FoldDirection.Up:
                    return Vector3.forward;
                case FoldDirection.Down:
                    return Vector3.back;
                case FoldDirection.Left:
                    return Vector3.left;
                case FoldDirection.Right:
                    return Vector3.right;
                default:
                    return Vector3.zero;
            }
        }

        private IEnumerator ShakeBlock(FoldingBlock block)
        {
            Vector3 originalPos = block.transform.position;
            float shakeTime = 0.3f;
            float elapsed = 0f;

            while (elapsed < shakeTime)
            {
                float x = Random.Range(-0.1f, 0.1f);
                float z = Random.Range(-0.1f, 0.1f);
                block.transform.position = originalPos + new Vector3(x, 0, z);

                elapsed += Time.deltaTime;
                yield return null;
            }

            block.transform.position = originalPos;
            block.transform.localScale = Vector3.one;
        }

    }
}