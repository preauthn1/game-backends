<?php

namespace App\Helpers;

class NumberHelper
{
    /**
     * 數字處理
     *
     * @param int $mode 模式 1=無條件捨去 2=無條件進位
     * @param float $number 數字
     * @param int $scale 小數位數
     * @param bool $isFormat 是否千分位逗號
     *
     */
    public static function formatDecimal(int $mode = 0, float $number = 0, int $scale = 0, bool $isFormat = false): string
    {
        if (empty($mode)) {
            return $number;
        }
        $calNumber = bcmul($number, pow(10, $scale));
        if (2 == $mode) {
            $calNumber++;
        }
        $result = bcdiv($calNumber, pow(10, $scale), $scale);
        if ($isFormat) {
            $result = number_format($result, $scale);
        }
        return $result;
    }
}
