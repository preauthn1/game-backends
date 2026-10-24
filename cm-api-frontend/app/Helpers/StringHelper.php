<?php

namespace App\Helpers;

class StringHelper
{
    /**
     * 字串隱碼
     *
     * @param string $string 需要隱碼的字串
     * @param int $start 顯示的開頭位數
     * @param int $end 顯示的結尾位數
     * @param int $maskLength 中間固定隱碼位數 0表示按string長度
     * @param string $mask 隱碼字元
     *
     */
    public static function stringEncrypt(string $string = '', int $start = 0, int $end = 0, int $maskLength = 0, string $mask = '*'): string
    {
        if (empty($string)) {
            return '';
        }
        $startString = '';
        if ($start != 0) {
            $startString = mb_substr($string, 0, $start);
        }
        if ($maskLength == 0) {
            $maskLength = mb_strlen($string) - ($start + $end);
        }
        $endString = '';
        if ($end != 0) {
            $endString = mb_substr($string, $end * -1);
        }
        return $startString . str_pad('', $maskLength, $mask) . $endString;
    }
}
