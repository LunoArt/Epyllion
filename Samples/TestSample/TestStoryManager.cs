using UnityEngine;

namespace Luno.Epyllion.Samples
{
    public class TestStoryManager : MonoBehaviour
    {
        private StorySceneManager manager;
        // Start is called before the first frame update
        void Start()
        {
            manager = GetComponent<StorySceneManager>();
            //manager.story.SetState(manager.story.GetInitialState());
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
