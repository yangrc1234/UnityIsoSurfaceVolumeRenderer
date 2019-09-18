using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;


public interface IAsyncReaderProgressNotifier {
    void Notify(string progress);
}

public class DescriptionFileReader : BaseAsyncVolumeReader {
    public class DescriptionFile {
        public string[] volumeData;
        public string albedoTex;
        public string metallicTex;
        public int width;
        public int height;
        public int depth;
        public int byteCount;
        public float gradientScale = 1.0f;
        public string relativePath;
        private static bool parseHeadline(DescriptionFile desc,string line,int lineNo) {
            var splitted = line.Split(new char[] { ':' },2);
            if (splitted.Length == 0)
                return false;   //nothing this line.
            if (splitted[0][0] == '=') {     //divide line, means the following lines are all volume data.
                return true;
            }
            if (splitted[0][1] == '#')       //commment line
                return false;
            var key = splitted[0].Trim().ToLower();   //key
            switch (key) {
                case "width":
                    if (!int.TryParse(splitted[1].Trim(), out desc.width))
                        throw new UnityException("Problem while parsing line " + lineNo);
                    break;
                case "height":
                    if (!int.TryParse(splitted[1].Trim(), out desc.height))
                        throw new UnityException("Problem while parsing line " + lineNo);
                    break;
                case "depth":
                    if (!int.TryParse(splitted[1].Trim(), out desc.depth))
                        throw new UnityException("Problem while parsing line " + lineNo);
                    break;
                case "bytecount":
                    if (!int.TryParse(splitted[1].Trim(), out desc.byteCount))
                        throw new UnityException("Problem while parsing line " + lineNo);
                    break;
                case "gradientscale":
                    if (!float.TryParse(splitted[1].Trim(), out desc.gradientScale))
                        throw new UnityException("Problem while parsing line " + lineNo);
                    break;
                case "albedo":
                    desc.albedoTex = splitted[1].Trim();
                    break;
                case "metallic":
                    desc.metallicTex = splitted[1].Trim();
                    break;
                default:
                    break;
            }

            return false;
        }
        public static DescriptionFile ParseFile(string path) {
            var res = new DescriptionFile();
            res.relativePath = path;
            using (FileStream headFileStream = new FileStream(path, FileMode.Open)) {
                if (!headFileStream.CanRead)
                    throw new UnityException("Not existed file!");
                var LineReader = new StreamReader(headFileStream);
                string line;
                List<string> volumeFile=new List<string>();
                bool headOver = false;
                int lineNo = 0;
                while ((line = LineReader.ReadLine())!=null) {
                    lineNo++;
                    if (!headOver) {
                        if (parseHeadline(res, line, lineNo))
                            headOver = true;
                    } else {
                        volumeFile.Add(line);
                    }
                }
                res.volumeData = volumeFile.ToArray();
            }
            return res;
        }
    }

    public class AsyncReader : ThreadedReadTexture {
        private VolumeDataInfo result;
        private DescriptionFile descFile;
        private IAsyncReaderProgressNotifier notifier;
        public AsyncReader(DescriptionFile descFile,IAsyncReaderProgressNotifier notifier) {
            this.descFile = descFile;
            this.notifier = notifier;
        }
        protected override void ThreadFunction() {
            this.result = DescriptionFileReader.ReadFromFile(descFile, notifier);
        }
        public override VolumeDataInfo GetTexture() {
            return result;
        }
    }

    [System.NonSerialized]
    public string _headFilePath;
    public DescriptionFile desc;
    protected override ThreadedReadTexture readTexture() {
        try {
            desc = DescriptionFile.ParseFile(_headFilePath);
            return new AsyncReader(desc, this);
        } catch (Exception e) {
            throw new UnityException(e.Message);
        }
    }

    public Texture2D GetAlbedoTransfer() {
        if (desc.albedoTex == null)
            return null;
        var res = new Texture2D(2, 2);
        try {
            res.LoadImage(File.ReadAllBytes(desc.relativePath + "/../" + desc.albedoTex));
            res.wrapMode = TextureWrapMode.Clamp;
        } catch (Exception) {
            return null;   
        }
        return res;
    }

    public Texture2D GetMetallicTransfer() {
        if (desc.albedoTex == null)
            return null;
        var res = new Texture2D(2, 2);
        try {
            res.LoadImage(File.ReadAllBytes(desc.relativePath + "/../" + desc.metallicTex));
            res.wrapMode = TextureWrapMode.Clamp;
        } catch (Exception) {
            return null;
        }
        return res;
    }

    private static int CeilToBinary(int num) {
        num -= 1;
        int i;
        for (i = 0; num > 0; i++) {
            num /= 2;
        }
        return 1 << i;
    }

    public static VolumeDataInfo ReadFromFile(DescriptionFile desc, IAsyncReaderProgressNotifier notifier) {

        var twidth = CeilToBinary(desc.width);
        var theight = CeilToBinary(desc.height);
        var tdepth = CeilToBinary(desc.depth);

        var fileData = new int[twidth * theight * tdepth];

        FileStream currentFile = null;
        bool flag = false;

        int? minData = null;
        int? maxData = null;
        int volumeFileIndex = 0;
        for (int z = 0; z < desc.depth; z++) {
            if (flag)
                break;
            for (int y = 0; y < desc.height; y++) {
                if (flag)
                    break;
                for (int x = 0; x < desc.width; x++) {
                    if (currentFile == null) {
                        if (volumeFileIndex < desc.volumeData.Length) { //we have next file
                            var nextFilePath = desc.volumeData[volumeFileIndex++];
                            currentFile = new FileStream(desc.relativePath + "/../" + nextFilePath, FileMode.Open);
                            if (notifier != null) {
                                notifier.Notify("Reading file.. : " + nextFilePath);
                            }
                        } else { //all input finished.

                            flag = true;
                            break;
                        }
                    }

                    //read it.
                    byte[] byData = new byte[desc.byteCount];
                    if (currentFile.Read(byData, 0, desc.byteCount) == 0) {
                        x--;
                        currentFile.Close();
                        currentFile = null;
                        continue;
                    }
                    int data = 0;
                    for (int i = 0; i < desc.byteCount; i++) {
                        data <<= 8;
                        data += byData[i];
                    }
                    fileData[x + y * twidth + z * twidth * theight] = data;
                    if (minData == null)
                        minData = data;
                    else {
                        minData = Mathf.Min(minData.Value, data);
                    }
                    if (maxData == null)
                        maxData = data;
                    else {
                        maxData = Mathf.Max(maxData.Value, data);
                    }
                }
            }
        }

        var result = new VolumeDataInfo(twidth, theight, tdepth);
        for (int i = 0; i < twidth; i++) {
            notifier.Notify("Filling texture...(" + (int)((float)i / twidth * 100)+"%)");
            for (int j = 0; j < theight; j++) {
                for (int k = 0; k < tdepth; k++) {
                    var data = (float)fileData[i + j * twidth + k * theight * twidth] / (maxData.Value - minData.Value);
                    result[i, j, k] = new Color(data, data, data, data);
                }
            }
        }
        return result;
    }
}
