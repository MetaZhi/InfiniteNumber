using UnityEngine;

namespace HongliuSchool
{
    public class InfiniteNumberDebug : MonoBehaviour
    {
        private InfiniteNumber a;
        private InfiniteNumber b;

        // Start is called before the first frame update
        void Start()
        {
            a = new InfiniteNumber(1923658);
            b = new InfiniteNumber(923658);

            var c = InfiniteNumber.Parse("7.18H");
            Debug.Log(c);
            Debug.Log(c.DebugString());

            Debug.Log(a);
            Debug.Log(a.DebugString());
            Debug.Log(b);
            Debug.Log(b.DebugString());
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                a = a + a;
                Debug.Log(a);
                Debug.Log(a.DebugString());
            }
        }
    }
}