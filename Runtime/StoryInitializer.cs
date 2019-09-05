using System.Collections;
using UnityEngine;

namespace Luno.Epyllion
{
    public class StoryInitializer : MonoBehaviour
    {
        public Story story;

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();
            
            story.SetState(story.CalculateInitialState());
        }
    }
}