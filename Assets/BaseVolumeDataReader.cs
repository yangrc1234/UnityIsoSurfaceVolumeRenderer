using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class VolumeDataInfo {
    public VolumeDataInfo(int width, int height, int thickness) {
        this.height = height;
        this.width = width;
        this.thickness = thickness;
        data = new Color[width * height * thickness];
    }
    public Color[] data;
    public int width;
    public int height;
    public int thickness;
    public Color this[int x, int y, int z] {
        get {
            if (x < 0 || y < 0 || z < 0 || x >= width || y >= height || z >= thickness) {
                return new Color(0, 0, 0, 0);
            }
            return data[z * height * width + y * width + x];
        }
        set {
            if (x < 0 || y < 0 || z < 0 || x >= width || y >= height || z >= thickness) {
                return;
            }
            data[z * height * width + y * width + x] = value;
        }
    }
}