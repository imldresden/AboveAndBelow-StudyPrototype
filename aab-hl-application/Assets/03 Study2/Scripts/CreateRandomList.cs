using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;

public class CreateRandomList : MonoBehaviour
{
    [Tooltip("Has to be even (i have not implemented an odd distinction case)")]
    [SerializeField]
    private int participantsNumber;
    [SerializeField]
    private int _repeats = 1;
    [ContextMenu("WriteFile")]
    public void WriteFile()
    {
        List<string> linesList = new List<string>();
        List<int[]> partLatinSquare = new List<int[]>();
        int[] row1 = { 1, 2, 3 };
        int[] row2 = { 3, 1, 2 };
        int[] row3 = { 2, 3, 1 };
        int[] row4 = { 3, 2, 1 };
        int[] row5 = { 1, 3, 2 };
        int[] row6 = { 2, 1, 3 };
        partLatinSquare.Add(row1);
        partLatinSquare.Add(row2);
        partLatinSquare.Add(row3);
        partLatinSquare.Add(row4);
        partLatinSquare.Add(row5);
        partLatinSquare.Add(row6);

        List<string> randoms = new List<string>() 
        {
            "1-I",
            "1-T",
            "2-I",
            "2-T",
            "3-I",
            "3-T",
            "4-I",
            "4-T",
            "5-I",
            "5-T",
        };


        Debug.Log(partLatinSquare[1][0]);
        for (int participantCount = 0; participantCount < participantsNumber / 2; participantCount++)
        {
            List<List<string>> randomList1 = new List<List<string>>();
            for (int partsCount = 0; partsCount < 3; partsCount++)
            {
                List<string> tmpList = new List<string>();
                int partCount = partLatinSquare[participantCount % partLatinSquare.Count][partsCount];

                for (int _ = 0; _ < _repeats; _++)
                {
                    ShuffleExtension.Shuffle(randoms);
                    for (int contentInstanceCount = 0; contentInstanceCount < randoms.Count; contentInstanceCount++)
                    {
                        string contentInstance = randoms[contentInstanceCount % randoms.Count];

                        tmpList.Add(
                            $"C-P{partCount}-{contentInstance}"
                        );
                    }

                    ShuffleExtension.Shuffle(randoms);
                    for (int contentInstanceCount = 0; contentInstanceCount < randoms.Count; contentInstanceCount++)
                    {
                        string contentInstance = randoms[contentInstanceCount % randoms.Count];

                        tmpList.Add(
                            $"F-P{partCount}-{contentInstance}"
                        );
                    }
                }

                randomList1.Add(tmpList);
            }

            List<List<string>> randomList2 = new List<List<string>>();
            for (int partsCount = 0; partsCount < 3; partsCount++)
            {
                List<string> tmpList = new List<string>();
                int partCount = partLatinSquare[participantCount % partLatinSquare.Count][partsCount];

                for (int _ = 0; _ < _repeats; _++)
                {
                    ShuffleExtension.Shuffle(randoms);
                    for (int contentInstanceCount = 0; contentInstanceCount < randoms.Count; contentInstanceCount++)
                    {
                        string contentInstance = randoms[contentInstanceCount % randoms.Count];

                        tmpList.Add(
                            $"F-P{partCount}-{contentInstance}"
                        );
                    }

                    ShuffleExtension.Shuffle(randoms);
                    for (int contentInstanceCount = 0; contentInstanceCount < randoms.Count; contentInstanceCount++)
                    {
                        string contentInstance = randoms[contentInstanceCount % randoms.Count];

                        tmpList.Add(
                            $"C-P{partCount}-{contentInstance}"
                        );
                    }
                }

                randomList2.Add(tmpList);
            }

            string randomString1 = String.Join("+", randomList1.Select(rl => String.Join(",", rl)));
            string randomString2 = String.Join("+", randomList2.Select(rl => String.Join(",", rl)));

            string part1 = partLatinSquare[participantCount % partLatinSquare.Count][0].ToString();
            string part2 = partLatinSquare[participantCount % partLatinSquare.Count][1].ToString();
            string part3 = partLatinSquare[participantCount % partLatinSquare.Count][2].ToString();

            linesList.Add(
                $"{part1},{part2},{part3},C,F|{randomString1}"
            );
            linesList.Add(
                $"{part1},{part2},{part3},F,C|{randomString2}"
            );
        }

        string[] lines = linesList.ToArray();
        File.WriteAllLines("./Assets/03 Study1/RandomList/List.txt", lines);
        //finished file in following format: each line represents one participant. Each line has the format "Partnumber,Partnumber,Partnumber,CorF,CorF|PartAVariableDescriptors+PartBVariableDescriptors+PartCVariableDesciptors"
        // each variable descriptor has the format "ForC-PPartnumber-UsedValueNumber-IorT"
    }


}

//shuffle
static class ShuffleExtension
{
    public static void Shuffle<T>(this IList<T> list)
    {
        RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
        int n = list.Count;
        while (n > 1)
        {
            byte[] box = new byte[1];
            do provider.GetBytes(box);
            while (!(box[0] < n * (Byte.MaxValue / n)));
            int k = (box[0] % n);
            n--;
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
