<?php

namespace App\Enums;

class ValidateTokenEnum
{
    //Type
    const TYPE_DATA_VERIFY = 1; //資料驗證
    const TYPE_FORGET_PASSWORD = 2; //忘記密碼
    const TYPE_RETRIEVE_PASSWORD = 3; //找回密碼
    const TYPE_WITHDRAWAL_VERIFY = 4; //洗分驗證
    const TYPE_PAYMENT_PASSWORD = 5; //支付密碼
    const TYPE_FORGET_PAYMENT_PASSWORD = 6; //忘記支付密碼
    //const TYPE_WITHDRAWAL_VERIFY = 7; //洗分驗證
    const TYPE_ONE_PAGE_REGISTRATION = 8; //一頁註冊

    //verify_by
    const VERIFY_MOBILE = 1;  //手機
    const VERIFY_EMAIL = 2;   //e-mail
}