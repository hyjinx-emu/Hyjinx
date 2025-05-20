using Hyjinx.Common.Collections;

namespace Hyjinx.Tests.Collections
{
    class TreeDictionaryTests
    {
        [Test]
        public void EnsureAddIntegrity()
        {
            TreeDictionary<int, int> dictionary = new();

            ClassicAssert.AreEqual(dictionary.Count, 0);

            dictionary.Add(2, 7);
            dictionary.Add(1, 4);
            dictionary.Add(10, 2);
            dictionary.Add(4, 1);
            dictionary.Add(3, 2);
            dictionary.Add(11, 2);
            dictionary.Add(5, 2);

            ClassicAssert.AreEqual(dictionary.Count, 7);

            List<KeyValuePair<int, int>> list = dictionary.AsLevelOrderList();

            /*
             *  Tree Should Look as Follows After Rotations
             *  
             *        2
             *    1        4
             *           3    10
             *              5    11
             *  
             */

            ClassicAssert.AreEqual(list.Count, dictionary.Count);
            ClassicAssert.AreEqual(list[0].Key, 2);
            ClassicAssert.AreEqual(list[1].Key, 1);
            ClassicAssert.AreEqual(list[2].Key, 4);
            ClassicAssert.AreEqual(list[3].Key, 3);
            ClassicAssert.AreEqual(list[4].Key, 10);
            ClassicAssert.AreEqual(list[5].Key, 5);
            ClassicAssert.AreEqual(list[6].Key, 11);
        }

        [Test]
        public void EnsureRemoveIntegrity()
        {
            TreeDictionary<int, int> dictionary = new();

            ClassicAssert.AreEqual(dictionary.Count, 0);

            dictionary.Add(2, 7);
            dictionary.Add(1, 4);
            dictionary.Add(10, 2);
            dictionary.Add(4, 1);
            dictionary.Add(3, 2);
            dictionary.Add(11, 2);
            dictionary.Add(5, 2);
            dictionary.Add(7, 2);
            dictionary.Add(9, 2);
            dictionary.Add(8, 2);
            dictionary.Add(13, 2);
            dictionary.Add(24, 2);
            dictionary.Add(6, 2);
            ClassicAssert.AreEqual(dictionary.Count, 13);

            List<KeyValuePair<int, int>> list = dictionary.AsLevelOrderList();

            /*
             *  Tree Should Look as Follows After Rotations
             *  
             *              4
             *      2               10
             *  1      3       7         13
             *              5      9  11    24
             *                6  8 
             */

            foreach (KeyValuePair<int, int> node in list)
            {
                Console.WriteLine($"{node.Key} -> {node.Value}");
            }
            ClassicAssert.AreEqual(list.Count, dictionary.Count);
            ClassicAssert.AreEqual(list[0].Key, 4);
            ClassicAssert.AreEqual(list[1].Key, 2);
            ClassicAssert.AreEqual(list[2].Key, 10);
            ClassicAssert.AreEqual(list[3].Key, 1);
            ClassicAssert.AreEqual(list[4].Key, 3);
            ClassicAssert.AreEqual(list[5].Key, 7);
            ClassicAssert.AreEqual(list[6].Key, 13);
            ClassicAssert.AreEqual(list[7].Key, 5);
            ClassicAssert.AreEqual(list[8].Key, 9);
            ClassicAssert.AreEqual(list[9].Key, 11);
            ClassicAssert.AreEqual(list[10].Key, 24);
            ClassicAssert.AreEqual(list[11].Key, 6);
            ClassicAssert.AreEqual(list[12].Key, 8);

            list.Clear();

            dictionary.Remove(7);

            /*
             *  Tree Should Look as Follows After Removal
             *  
             *              4
             *      2               10
             *  1      3       6         13
             *              5      9  11    24
             *                  8 
             */

            list = dictionary.AsLevelOrderList();
            foreach (KeyValuePair<int, int> node in list)
            {
                Console.WriteLine($"{node.Key} -> {node.Value}");
            }
            ClassicAssert.AreEqual(list[0].Key, 4);
            ClassicAssert.AreEqual(list[1].Key, 2);
            ClassicAssert.AreEqual(list[2].Key, 10);
            ClassicAssert.AreEqual(list[3].Key, 1);
            ClassicAssert.AreEqual(list[4].Key, 3);
            ClassicAssert.AreEqual(list[5].Key, 6);
            ClassicAssert.AreEqual(list[6].Key, 13);
            ClassicAssert.AreEqual(list[7].Key, 5);
            ClassicAssert.AreEqual(list[8].Key, 9);
            ClassicAssert.AreEqual(list[9].Key, 11);
            ClassicAssert.AreEqual(list[10].Key, 24);
            ClassicAssert.AreEqual(list[11].Key, 8);

            list.Clear();

            dictionary.Remove(10);

            list = dictionary.AsLevelOrderList();
            /*
             *  Tree Should Look as Follows After Removal
             *  
             *              4
             *      2               9
             *  1      3       6         13
             *              5      8  11    24
             *                   
             */
            foreach (KeyValuePair<int, int> node in list)
            {
                Console.WriteLine($"{node.Key} -> {node.Value}");
            }
            ClassicAssert.AreEqual(list[0].Key, 4);
            ClassicAssert.AreEqual(list[1].Key, 2);
            ClassicAssert.AreEqual(list[2].Key, 9);
            ClassicAssert.AreEqual(list[3].Key, 1);
            ClassicAssert.AreEqual(list[4].Key, 3);
            ClassicAssert.AreEqual(list[5].Key, 6);
            ClassicAssert.AreEqual(list[6].Key, 13);
            ClassicAssert.AreEqual(list[7].Key, 5);
            ClassicAssert.AreEqual(list[8].Key, 8);
            ClassicAssert.AreEqual(list[9].Key, 11);
            ClassicAssert.AreEqual(list[10].Key, 24);
        }

        [Test]
        public void EnsureOverwriteIntegrity()
        {
            TreeDictionary<int, int> dictionary = new();

            ClassicAssert.AreEqual(dictionary.Count, 0);

            dictionary.Add(2, 7);
            dictionary.Add(1, 4);
            dictionary.Add(10, 2);
            dictionary.Add(4, 1);
            dictionary.Add(3, 2);
            dictionary.Add(11, 2);
            dictionary.Add(5, 2);
            dictionary.Add(7, 2);
            dictionary.Add(9, 2);
            dictionary.Add(8, 2);
            dictionary.Add(13, 2);
            dictionary.Add(24, 2);
            dictionary.Add(6, 2);
            ClassicAssert.AreEqual(dictionary.Count, 13);

            List<KeyValuePair<int, int>> list = dictionary.AsLevelOrderList();

            foreach (KeyValuePair<int, int> node in list)
            {
                Console.WriteLine($"{node.Key} -> {node.Value}");
            }

            /*
             *  Tree Should Look as Follows After Rotations
             *  
             *              4
             *      2               10
             *  1      3       7         13
             *              5      9  11    24
             *                6  8 
             */

            ClassicAssert.AreEqual(list.Count, dictionary.Count);
            ClassicAssert.AreEqual(list[0].Key, 4);
            ClassicAssert.AreEqual(list[1].Key, 2);
            ClassicAssert.AreEqual(list[2].Key, 10);
            ClassicAssert.AreEqual(list[3].Key, 1);
            ClassicAssert.AreEqual(list[4].Key, 3);
            ClassicAssert.AreEqual(list[5].Key, 7);
            ClassicAssert.AreEqual(list[6].Key, 13);
            ClassicAssert.AreEqual(list[7].Key, 5);
            ClassicAssert.AreEqual(list[8].Key, 9);
            ClassicAssert.AreEqual(list[9].Key, 11);
            ClassicAssert.AreEqual(list[10].Key, 24);
            ClassicAssert.AreEqual(list[11].Key, 6);
            ClassicAssert.AreEqual(list[12].Key, 8);

            ClassicAssert.AreEqual(list[4].Value, 2);

            dictionary.Add(3, 4);

            list = dictionary.AsLevelOrderList();

            ClassicAssert.AreEqual(list[4].Value, 4);


            // Assure that none of the nodes locations have been modified.
            ClassicAssert.AreEqual(list[0].Key, 4);
            ClassicAssert.AreEqual(list[1].Key, 2);
            ClassicAssert.AreEqual(list[2].Key, 10);
            ClassicAssert.AreEqual(list[3].Key, 1);
            ClassicAssert.AreEqual(list[4].Key, 3);
            ClassicAssert.AreEqual(list[5].Key, 7);
            ClassicAssert.AreEqual(list[6].Key, 13);
            ClassicAssert.AreEqual(list[7].Key, 5);
            ClassicAssert.AreEqual(list[8].Key, 9);
            ClassicAssert.AreEqual(list[9].Key, 11);
            ClassicAssert.AreEqual(list[10].Key, 24);
            ClassicAssert.AreEqual(list[11].Key, 6);
            ClassicAssert.AreEqual(list[12].Key, 8);
        }
    }
}