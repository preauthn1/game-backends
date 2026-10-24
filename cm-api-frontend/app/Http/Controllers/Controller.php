<?php

namespace App\Http\Controllers;

abstract class Controller
{
    protected array $validatorMessage;

    public function getPagination($total, $pre_page, $page)
    {
        $pagination = array();
        $pagination['per_page'] = $pre_page;
        $pagination['total'] = $total;
        $pages = ceil($total / $pre_page);
        $pagination['pages'] = $pages;
        $pagination['previous'] = ($page != 1 && $total > 0) ? ($page - 1) : '';
        $pagination['current'] = $page;
        $pagination['next'] = ($pages > $page) ? ($page + 1) : '';
        return $pagination;
    }

    public function __construct()
    {
        //定義參數驗證錯誤信息
        $this->validatorMessage = [
            'avatar_id.required' => __('error_message.3001'),
            'avatar_id.digits_between' => __('error_message.4001'),

            'email.required' => __('error_message.3009'),
            'email.email' => __('error_message.5006'),

            'username.required' => __('error_message.3002'),
            'username.between' => __('error_message.5007'),
            'username.unique' => __('error_message.1101'),
            'username.regex' => __('error_message.50071'),

            'password.required' => __('error_message.3003'),
            'password.between' => __('error_message.5008'),
            'password.confirmed' => __('error_message.5009'),
            'password.regex' => __('error_message.50081'),

            'old_password.required' => __('error_message.3011'),
            'old_password.string' => __('error_message.5004'),
            'old_password.between' => __('error_message.5004'),

            'new_password.required' => __('error_message.3012'),
            'new_password.string' => __('error_message.5013'),
            'new_password.between' => __('error_message.5013'),
            'new_password.confirmed' => __('error_message.5014'),

            'fund_password.required' => __('error_message.3013'),
            'fund_password.string' => __('error_message.5015'),
            'fund_password.between' => __('error_message.5015'),
            'fund_password.confirmed' => __('error_message.5016'),

            'old_fund_password.required' => __('error_message.3014'),
            'old_fund_password.string' => __('error_message.5005'),
            'old_fund_password.between' => __('error_message.5005'),

            'new_fund_password.required' => __('error_message.3015'),
            'new_fund_password.string' => __('error_message.5017'),
            'new_fund_password.between' => __('error_message.5017'),
            'new_fund_password.confirmed' => __('error_message.5018'),

            'mobile.required' => __('error_message.3004'),
            'mobile.digits_between' => __('error_message.4002'),

            'sms_code.required' => __('error_message.3005'),
            'sms_code.between' => __('error_message.5002'),

            'referral_code.required' => __('error_message.3006'),
            'referral_code.between' => __('error_message.5010'),

            'captcha_key.required' => __('error_message.3007'),
            'captcha_key.between' => __('error_message.5011'),
            'captcha_code.required' => __('error_message.3008'),
            'captcha_code.between' => __('error_message.5001'),

            'name.required' => __('error_message.3010'),
            'name.string' => __('error_message.5012'),

            'bank_id.required' => __('error_message.3016'),
            'bank_id.digits_between' => __('error_message.5019'),

            'bank_account_id.required' => __('error_message.3026'),
            'bank_account_id.digits_between' => __('error_message.5030'),

            'account_name.required' => __('error_message.3017'),
            'account_name.string' => __('error_message.5020'),

            'account_no.required' => __('error_message.3018'),
            'account_no.string' => __('error_message.5021'),
            'account_no.between' => __('error_message.5021'),

            'crypto_address.required' => __('error_message.3019'),
            'crypto_address.string' => __('error_message.5022'),

            'id.required' => __('error_message.1084'),
            'id.digits_between' => __('error_message.1085'),

            'miner_id.required' => __('error_message.3020'),
            'miner_id.digits_between' => __('error_message.5023'),

            'miner_type.required' => __('error_message.3021'),
            'miner_type.digits_between' => __('error_message.5024'),

            'quantity.required' => __('error_message.3022'),
            'quantity.digits_between' => __('error_message.5025'),

            //payment - deposit
            'top_up_type.required' => __('error_message.8001'),
            'top_up_type.digits_between' => __('error_message.8002'),

            'mm_ids.required' => __('error_message.3023'),
            'mm_ids.array' => __('error_message.5027'),

            'mm_id.required' => __('error_message.3024'),
            'mm_id.digits_between' => __('error_message.5028'),

            'calculate_date.required' => __('error_message.3025'),
            'calculate_date.date' => __('error_message.5029'),

            'bank_tax_type.required' => __('error_message.3027'),
            'bank_tax_type.digits_between' => __('error_message.5031'),

            'bank_tax.required' => __('error_message.3028'),
            'bank_tax.numeric' => __('error_message.5032'),
            'bank_tax.between' => __('error_message.5032'),

            'domain.required' => __('error_message.3029'),
            'domain.domain' => __('error_message.5033'),

            'type.digits_between' => __('error_message.5035'),
        ];
    }
}
