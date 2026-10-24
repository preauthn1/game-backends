<?php

namespace App\Enums;

enum PublicEnum
{
    const ResponseCode = [
        'success' => 1000,
        'needLogin' => 9998,
    ];
    const Language = [
        'en' => [
            'id' => 1,
            'code' => 'en',
        ],
        'zh-tw' => [
            'id' => 2,
            'code' => 'zh-tw',
        ],
        'zh-cn' => [
            'id' => 3,
            'code' => 'zh-cn',
        ],
    ];
    const DefaultPage = 1;
    const DefaultPageSize = 10;
    const DefaultScale = 2;
    const DefaultExportPage = 1;
    const DefaultExportPageSize = 5000;
}
