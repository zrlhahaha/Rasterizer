# Soft Rasterizer
这是一个用unity做的软渲染器，结果输出到贴图中，目前还没有把视锥体裁剪加入到管线中，所以如果有物体在摄像机后面的话会被透视矩阵变换到前面去，从而导致不正确的渲染结果，以下图片为运行结果
![image](https://github.com/zrlhahaha/Rasterizer/blob/master/README_IMG_1.png)
![image](https://github.com/zrlhahaha/Rasterizer/blob/master/README_IMG_2.png)
![image](https://github.com/zrlhahaha/Rasterizer/blob/master/README_IMG_3.png)

场景简单的话，256x144，可以有个30，40帧
![image](https://github.com/zrlhahaha/Rasterizer/blob/master/README_IMG_4.gif)
