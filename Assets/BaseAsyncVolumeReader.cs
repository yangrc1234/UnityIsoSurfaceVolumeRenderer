using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public abstract class ThreadedReadTexture : ThreadJob {
    public abstract VolumeDataInfo GetTexture();
}

public class AsyncGradientCalculator : ThreadedReadTexture {
    VolumeDataInfo info;
    VolumeDataInfo res;
    IAsyncReaderProgressNotifier notifier;
    public AsyncGradientCalculator(VolumeDataInfo info,IAsyncReaderProgressNotifier notifier) {
        this.info = info;
        this.notifier = notifier;
    }

    protected override void ThreadFunction() {
        res = new VolumeDataInfo(info.width, info.height, info.thickness);
        for (int i = 0; i < info.width; i++) {
            notifier.Notify("Gradient calculating.." + (int)(100 * ((float)i / info.width)) + "%");
            for (int j = 0; j < info.height; j++) {;
                for (int k = 0; k < info.thickness; k++) {
                    res[i, j, k] = GetGradientSobel(info, i, j, k);
                }
            }
        }
    }

    private Color GetGradientSobel(VolumeDataInfo tex, int x, int y, int z) {
        Color result = new Color();
        float[,,] kernal = {
        {
                { -1,-2,-1 },
                { -2,-4,-2 },
                { -1,-2,-1 }

            },{
                { 1,2,1 },
                { 2,4,2 },
                { 1,2,1 }
            }
        };
        for (int i = 0; i < 2; i++) {
            for (int j = 0; j < 3; j++) {
                for (int k = 0; k < 3; k++) {
                    result.r += kernal[i, j, k] * tex[x + i * 2 - 1, y + j - 1, z + k - 1].r;
                }
            }
        }

        for (int i = 0; i < 2; i++) {
            for (int j = 0; j < 3; j++) {
                for (int k = 0; k < 3; k++) {
                    result.g += kernal[i, j, k] * tex[x + j - 1, y + i * 2 - 1, z + k - 1].r;
                }
            }
        }

        for (int i = 0; i < 2; i++) {
            for (int j = 0; j < 3; j++) {
                for (int k = 0; k < 3; k++) {
                    result.b += kernal[i, j, k] * tex[x + j - 1, y + k - 1, z + i * 2 - 1].r;
                }
            }
        }
        result /= 32;
        return result;
    }

    public override VolumeDataInfo GetTexture() {
        return res;
    }
}

public class BaseAsyncVolumeReader : MonoBehaviour,IAsyncReaderProgressNotifier {

    public bool finishedReading { get; set; }
    public bool finishedGradientCalculating { get; set; }
    public Texture3D tex {
        get {
            return _tex;
        }
    }
    public Texture3D gradientTex {
        get {
            return _gradientTex;
        }
    }
    private Texture3D _tex;
    private Texture3D _gradientTex;

    public void ImportTexture() {
        finishedGradientCalculating = false;
        finishedReading = false;
        var thread = readTexture();
        thread.Start();
        StartCoroutine(ReadMain(thread));
    }

    private IEnumerator ReadMain(ThreadedReadTexture thread) {
        yield return thread.WaitTillDone();
        var info = thread.GetTexture();
        _tex = new Texture3D(info.width, info.height, info.thickness, TextureFormat.RFloat, true);
        _tex.SetPixels(info.data);
        _tex.Apply(true, true);
        finishedReading = true;
        PrecalculateGradient(info);
    }

    private void  PrecalculateGradient(VolumeDataInfo info) {
        var t = new AsyncGradientCalculator(info,this);
        t.Start();
        StartCoroutine(ReadGradient(t));
    }

    private IEnumerator ReadGradient(AsyncGradientCalculator t) {
        yield return t.WaitTillDone();
        var info = t.GetTexture();
        _gradientTex = new Texture3D(info.width, info.height, info.thickness, TextureFormat.RGBAHalf, true);
        _gradientTex.SetPixels(info.data);
        _gradientTex.Apply(true, true);
        finishedGradientCalculating = true;
    }

    protected virtual ThreadedReadTexture readTexture() {
        return null;
    }

    public void Notify(string progress) {
        ThreadManager.instance.AddThreadCallback(
            () => {
                SystemController.instance.hint.hintText = progress;
            });
    }
}
