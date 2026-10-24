<?php

namespace App\Helpers;

use Illuminate\Support\Facades\Storage;

class CloudFlareHelper
{
    public static function uploadImage($image, $file_path = 'images')
    {
        $path = Storage::disk('r2')->put($file_path, $image);
        // 获取图片的完整 URL
        return Storage::disk('r2')->url($path);
    }

    public static function uploadBase64File($base64Image, $file_path = 'images')
    {
        // 去掉 base64 字符串的開頭部分 (例如 "data:image/png;base64,")
        $imageParts = explode(',', $base64Image);
        $image = base64_decode($imageParts[1]);

        // 確定圖像類型和文件擴展名
        $imageType = explode(';', explode(':', $imageParts[0])[1])[0];
        $extension = str_replace('image/', '', $imageType);

        // 生成唯一的文件名
        $fileName = uniqid() . '.' . $extension;
        $file_path .= '/' . $fileName;
        // 使用 R2 進行上傳
        Storage::disk('r2')->put($file_path, $image);

        // 返回上傳後的 URL
        return Storage::disk('r2')->url($file_path);
    }
}