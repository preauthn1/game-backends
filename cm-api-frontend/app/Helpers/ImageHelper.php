<?php

namespace App\Helpers;

//use SimpleSoftwareIO\QrCode\Facades\QrCode;
use BaconQrCode\Writer;
use BaconQrCode\Renderer\ImageRenderer;
use BaconQrCode\Renderer\Image\ImagickImageBackEnd;
use BaconQrCode\Renderer\Image\SvgImageBackEnd;
use BaconQrCode\Renderer\RendererStyle\RendererStyle;

class ImageHelper
{
    public static function generateQrCode($url, $size = 300)
    {
        $renderer = new ImageRenderer(
            new RendererStyle($size),
            new SvgImageBackEnd()
        );

        $writer = new Writer($renderer);
        $qrCodeImage = $writer->writeString($url);

        // 将二维码转换为 base64 编码的 PNG 图片
        $qrCodeBase64 = base64_encode($qrCodeImage);

        return "data:image/svg+xml;base64," . $qrCodeBase64;
    }
}