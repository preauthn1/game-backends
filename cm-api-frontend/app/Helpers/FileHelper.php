<?php

namespace App\Helpers;


class FileHelper
{

    public static function image2Base64($image)
    {
        // 读取图像内容
        $imageContents = file_get_contents($image->getPathname());

        // 将图像内容转换为 Base64 编码
        $base64Image = base64_encode($imageContents);

        // 获取文件的 MIME 类型
        $mimeType = $image->getClientMimeType();

        // 拼接完整的 Base64 图像字符串
        return 'data:' . $mimeType . ';base64,' . $base64Image;
    }
}