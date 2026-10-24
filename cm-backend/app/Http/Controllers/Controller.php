<?php

namespace App\Http\Controllers;

use App\Services\AgentOperateLogService;
use Illuminate\Support\Facades\Log;

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

    public function saveOperationLogData($class, $name, $request, $response, $start, $exception)
    {
        $data = [];
        $data['api_path'] = str_replace('App\\Http\\Controllers\\', '', $class ?? '');
        $data['api_name'] = $name ?? '';
        $data['request'] = $this->dataProcess($request ?? [], 'request');
        $data['response_code'] = $response->code ?? '';
        $data['response_message'] = $this->dataProcess($response->message ?? '', 'response_message');
        $data['response_data'] = $this->dataProcess($response->data ?? [], 'response_data');
        $data['time_cost'] = number_format(microtime(true) - ($start ?? microtime(true)), 4);
        $data['type'] = $exception ? 2 : 1;

        $agentOperateLogService = new AgentOperateLogService();
        $agentOperateLogService->create($data);
    }

    private function dataProcess($data, $type = '')
    {
        if (empty($data ?? [])) {
            return '';
        }
        if (!is_string($data)) {
            $jsonData = json_encode($data, JSON_UNESCAPED_UNICODE);
        } else {
            $jsonData = $data;
        }
        $length = strlen($jsonData);
        //資料超過10K就壓縮
        if ($length > env('OPERATION_LOG_DATA_MAX', 10 * 1024)) {
            if ($length > 10 * 1024 * 1024) {
                //資料超過10M紀錄Log
                Log::warning($type . ' data length over 10M');
            }
            return base64_encode(gzcompress($jsonData));
        } else {
            return $jsonData;
        }
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
            'username.string' => __('error_message.5007'),
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
            'bank_id.integer' => __('error_message.5019'),
            'bank_id.between' => __('error_message.5019'),

            'bank_account_id.required' => __('error_message.3026'),
            'bank_account_id.digits_between' => __('error_message.5039'),
            'account_id.required' => __('error_message.3026'),
            'account_id.digits_between' => __('error_message.5039'),

            'account_name.required' => __('error_message.3017'),
            'account_name.string' => __('error_message.5020'),
            'account_name.between' => __('error_message.5020'),

            'account_no.required' => __('error_message.3018'),
            'account_no.string' => __('error_message.5021'),
            'account_no.between' => __('error_message.5021'),

            'crypto_address.required' => __('error_message.3019'),
            'crypto_address.string' => __('error_message.5022'),

            'id.required' => __('error_message.1084'),
            'id.digits_between' => __('error_message.1085'),

            'miner_id.required' => __('error_message.3020'),
            'miner_id.digits_between' => __('error_message.5023'),
            'miner_id.not_in' => __('error_message.5023'),

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

            'login_ip.fuzzy_ip' => __('error_message.5036'),

            'register_ip.fuzzy_ip' => __('error_message.5036'),

            'currency_id.required' => __('error_message.3025'),
            'currency_id.digits_between' => __('error_message.5037'),
            'currency_id.integer' => __('error_message.5037'),
            'currency_id.between' => __('error_message.5037'),

            'account_alias.between' => __('error_message.5038'),

            'country_id.digits_between' => __('error_message.5040'),
            'country_id.min' => __('error_message.5040'),
            'country_id.not_in' => __('error_message.5040'),

            'role_id.required' => __('error_message.3027'),
            'role_id.digits_between' => __('error_message.5042'),

            'parent_id.required' => __('error_message.3028'),
            'parent_id.digits_between' => __('error_message.5043'),

            'menu_name.required' => __('error_message.3029'),
            'menu_name.string' => __('error_message.5044'),
            'menu_name.between' => __('error_message.5044'),

            'order.required' => __('error_message.3030'),
            'order.digits_between' => __('error_message.5045'),
            'order.integer' => __('error_message.5045'),
            'order.between' => __('error_message.5045'),

            'is_show.required' => __('error_message.3031'),
            'is_show.integer' => __('error_message.5046'),

            'url.string' => __('error_message.5047'),
            'url.between' => __('error_message.5047'),

            'define_id.required' => __('error_message.3032'),
            'define_id.digits_between' => __('error_message.5049'),

            'parent_define_id.required' => __('error_message.3033'),
            'parent_define_id.digits_between' => __('error_message.5048'),

            'group_type.required' => __('error_message.3034'),
            'group_type.digits_between' => __('error_message.5050'),

            'manager_name.string' => __('error_message.5051'),
            'manager_name.between' => __('error_message.5051'),

            'block_height.required' => __('error_message.3038'),
            'block_height.integer' => __('error_message.5052'),

            'block_time.required' => __('error_message.3039'),
            'block_time.date_format' => __('error_message.5053'),

            'mining_difficulty.required' => __('error_message.3040'),
            'mining_difficulty.numeric' => __('error_message.5054'),

            'mining_change.required' => __('error_message.3041'),
            'mining_change.numeric' => __('error_message.5055'),

            'block_reward.required' => __('error_message.3042'),
            'block_reward.numeric' => __('error_message.5056'),

            'amount.required' => __('error_message.3043'),
            'amount.numeric' => __('error_message.5057'),
            'amount.between' => __('error_message.5057'),

            'rate.required' => __('error_message.3044'),
            'rate.numeric' => __('error_message.5058'),
            'rate.between' => __('error_message.5058'),

            'bank_tax_type.required' => __('error_message.3045'),
            'bank_tax_type.digits_between' => __('error_message.5059'),

            'bank_tax.required' => __('error_message.3046'),
            'bank_tax.numeric' => __('error_message.5060'),
            'bank_tax.between' => __('error_message.5060'),

            'gauth_code.required' => __('error_message.3047'),
            'gauth_code.string' => __('error_message.5061'),
            'gauth_code.between' => __('error_message.5061'),

            'bank_account.required' => __('error_message.3017'),
            'bank_account.string' => __('error_message.5020'),

            'type_id.required' => __('error_message.3048'),
            'type_id.between' => __('error_message.5062'),

            'bank_name.required' => __('error_message.3049'),
            'bank_name.string' => __('error_message.5063'),
            'bank_name.between' => __('error_message.5063'),

            'bank_code.required' => __('error_message.3052'),
            'bank_code.string' => __('error_message.5038'),
            'bank_code.between' => __('error_message.5038'),

            'min_amount.required' => __('error_message.3050'),
            'min_amount.numeric' => __('error_message.5064'),
            'min_amount.between' => __('error_message.5064'),

            'max_amount.required' => __('error_message.3051'),
            'max_amount.numeric' => __('error_message.5065'),
            'max_amount.between' => __('error_message.5065'),

            'sort.digits_between' => __('error_message.5045'),

            'create_time_start.date_format' => __('error_message.5066'),
            'create_time_end.date_format' => __('error_message.5066'),
            'create_time_end.after_or_equal' => __('error_message.5066'),
            'check_time_start.date_format' => __('error_message.5068'),
            'check_time_end.date_format' => __('error_message.5069'),
            'check_time_end.after_or_equal' => __('error_message.5069'),

            'experience_commission.required' => __('error_message.3053'),
            'experience_commission.numeric' => __('error_message.5070'),
            'experience_commission.between' => __('error_message.5070'),

            'type.digits_between' => __('error_message.5062'),

            'isoCode.required' => __('error_message.3055'),
            'isoCode.string' => __('error_message.5071'),
            'isoCode.between' => __('error_message.5071'),

            'countryCode.required' => __('error_message.3056'),
            'countryCode.string' => __('error_message.5072'),
            'countryCode.between' => __('error_message.5072'),

            'status.required' => __('error_message.3054'),
            'status.integer' => __('error_message.5035'),
            'status.in' => __('error_message.5035'),

            'countryName.required' => __('error_message.3057'),
            'countryName.string' => __('error_message.5073'),
            'countryName.between' => __('error_message.5073'),

            'phoneRule.required' => __('error_message.3058'),
            'phoneRule.string' => __('error_message.5074'),
            'phoneRule.between' => __('error_message.5074'),
        ];
    }
}
