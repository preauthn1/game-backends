<?php

namespace App\Services;

use Illuminate\Support\Carbon;
use Illuminate\Support\Facades\Hash;
use App\Helpers\Common;
use App\Helpers\StringHelper;
use App\Repositories\MemberRepository;
use App\Repositories\MemberIdCardRepository;
use App\Repositories\MemberAccessTokenRepository;
use App\Repositories\MemberWalletRepository;
use App\Repositories\MemberMinerRepository;
use App\Repositories\MemberMinerRebateRepository;
use App\Repositories\MemberMinerRewardRepository;
use App\Repositories\MemberBankAccountRepository;
use App\Repositories\MemberCryptoAccountRepository;
use App\Repositories\TransactionLogRepository;
use App\Repositories\DepositOrderRepository;
use App\Repositories\WithdrawalOrderRepository;
use App\Enums\WalletTypeEnum;
use App\Enums\TimeEnum;
use App\Enums\WithdrawalEnum;
use Exception;
use DB;

class MemberService extends BaseService
{
    public function __construct()
    {
        $this->repository = new MemberRepository();
    }

    public function find($member_id, $check_status = false)
    {
        $member = $this->repository->find($member_id);
        //檢查帳號是否存在
        if (Common::IsNullOrEmptyString($member)) {
            throw new Exception(__('error_message.1201'), 1201);
        }
        if ($check_status) $this->checkMemberLock($member);

        $member->id_card_front = null;
        $member->id_card_back = null;
        $member->member_id_card_status = null;
        $member->member_id_card_statusTxt = __('param.member_id_card.status.9');
        if ($member->member_id_card_id > 0) {
            $memberIdCardRepository = new MemberIdCardRepository();
            $idCard = $memberIdCardRepository->find($member->member_id_card_id);
            $member->member_id_card_status = $idCard->status;
            $member->member_id_card_statusTxt = __('param.member_id_card.status.' . $idCard->status);

            if ($idCard->status == 1) {
                $member->id_card_front = $idCard->image_front;
                $member->id_card_back = $idCard->image_back;
            }
        }

        return $member;
    }

    public function view($member_id)
    {
        //充值總額、提現總額、餘額帳變紀錄、充值明細、提現明細、身分證資料、登入IP日誌、操作日誌
        $member = $this->find($member_id);
        unset($member->password);
        unset($member->fund_password);

        //充值總額、提現總額、
        $transactionLogRepository = new TransactionLogRepository();
        $transactionDeposit = $transactionLogRepository->getTotalPointByType($member_id, 1);
        $transactionWithdrawal = $transactionLogRepository->getTotalPointByType($member_id, 2);
        $member->total_deposit = $transactionDeposit->point;
        $member->total_withdrawal = $transactionWithdrawal->point;

        return $member;
    }

    public function findByUsername($username, $check_status = false, $hierarchy_path = '')
    {
        $member = $this->repository->findByUsername($username, $hierarchy_path);
        if ($check_status) {
            //檢查帳號是否存在
            if (Common::IsNullOrEmptyString($member)) {
                throw new Exception(__('error_message.1201'), 1201);
            }
        }
        return $member;
    }

    //檢查玩家是否存在
    public function checkMemberExistsByUsername($username)
    {
        $member = $this->repository->findByUsername($username);
        //檢查帳號是否存在
        if (Common::IsNotNullOrEmptyString($member)) {
            throw new Exception(__('error_message.1101'), 1101);
        }
    }

    public function checkByUsername($username)
    {
        return $this->repository->findByUsername($username);
    }

    public function findByMobile($mobile, $check_status = false)
    {
        $member = $this->repository->findByMobile($mobile);
        if ($check_status) $this->checkMemberLock($member);
        return $member;
    }

    private function checkMemberLock($member)
    {
        //檢查帳號是否鎖定
        if ($member->status != 1) {
            throw new Exception(__('error_message.9006'), 9006);
        }
    }

    public function getInviteCode()
    {
        return $this->repository->getInviteCode();
    }

    public function getMemberCount($params)
    {
        return $this->repository->getMemberCount($params);
    }

    public function getTodayRegistrationCount($params)
    {
        return $this->repository->getTodayRegistrationCount($params);
    }

    public function createVirtual($params)
    {
        //玩家
        $data = array();
        $data['level'] = 1;
        $data['agent_id'] = ($params->HOST_ID > 0 ? $params->HOST_ID : $params->ID);
        if (isset($params->parent_username)) {
            $parent = $this->findByUsername($params->parent_username);
            //檢查帳號是否存在
            if (Common::IsNullOrEmptyString($parent)) {
                throw new Exception(__('error_message.5041'), 5041);
            }
            if (in_array($parent->type, [1, 2, 3])) {
                $data['referral_code'] = $parent->invitation_code;
                $data['agent_id'] = $parent->agent_id;
                $data['level'] = $parent->level + 1;
            }
        }

        $data['uid'] = $this->repository->getUid();
        $data['type'] = $params->type;  //玩家類型; 1:正式會員, 2:內部測試會員, 3:外部測試會員, 9:特殊帳號(註冊用)
        $data['username'] = $params->username;
        $data['real_name'] = $params->real_name;
        $data['password'] = Hash::make($params->password);
        $data['fund_password'] = Hash::make($params->fund_password);
        $data['invitation_code'] = $this->repository->getInvitationCode();
        $data['country_id'] = $params->country_id;
        $data['register_country_id'] = $params->country_id;
        $data['is_validate'] = 1;
        $data['mobile'] = $params->mobile;
        $data['mobile_validate'] = 1;
        $data['status'] = 1;
        $member = $this->repository->create($data);
        if (!$member) {
            throw new Exception(__('error_message.2001'), 2001);
        }

        //玩家錢包
        $memberWalletRepository = new MemberWalletRepository();
        $data = array();
        $data['member_id'] = $member->id;
        $data['wallet_type_id'] = 1;
        $data['balance'] = 0;
        $data['status'] = 1;
        $memberWallet = $memberWalletRepository->create($data);
        if (!$memberWallet) {
            throw new Exception(__('error_message.2001'), 2001);
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
        return $this->updateById($member_id, $data);
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

    public function updateById($id, $data)
    {
        $result = $this->repository->updateById($id, $data);
        if (!$result) {
            throw new Exception(__('error_message.2001'), 2001);
        }
        return $result;
    }

    public function updateInfo($params)
    {
        $data = array();
        if (Common::IsNotNullOrEmptyString($params->real_name)) {
            $data['real_name'] = $params->real_name;
        }
        if (Common::IsNotNullOrEmptyString($params->country_id)) {
            $data['country_id'] = $params->country_id;
        }
        if (Common::IsNotNullOrEmptyString($params->mobile)) {
            $data['mobile'] = $params->mobile;
            $data['mobile_validate'] = 0;
        }
        if (Common::IsNotNullOrEmptyString($params->email)) {
            $data['email'] = $params->email;
            $data['email_validate'] = 0;
        }
        if (Common::IsNotNullOrEmptyString($params->password)) {
            $data['password'] = Hash::make($params->password);
        }
        if (Common::IsNotNullOrEmptyString($params->fund_password)) {
            $data['fund_password'] = Hash::make($params->fund_password);
        }
        if (isset($params->memo)) {
            $data['memo'] = $params->memo;
        }
        if (count($data) > 0) {
            $result = $this->updateById($params->member_id, $data);
            if (!$result) {
                throw new Exception(__('error_message.2001'), 2001);
            }
        }

        // 是否有變更銀行帳戶資訊
        $bankInfoChanged =
            Common::IsNotNullOrEmptyString($params->bank_id) ||
            Common::IsNotNullOrEmptyString($params->bank_account) ||
            Common::IsNotNullOrEmptyString($params->bank_no);

        if ($bankInfoChanged) {
            $memberBankAccountRepository = new MemberBankAccountRepository();
            //將全部status改為0
            $memberBankAccountRepository->setDisabled($params->member_id);
            $result = $memberBankAccountRepository->insertOrUpdateData(
                $params->member_id,
                $params->bank_id,
                $params->bank_account,
                $params->bank_no
            );
            if (!$result) {
                throw new Exception(__('error_message.2001'), 2001);
            }
        }

        //加密錢包地址
        // if (Common::IsNotNullOrEmptyString($params->crypto_id)) {

        // 是否有變更加密錢包地址
        $cryptoInfoChanged =
            Common::IsNotNullOrEmptyString($params->crypto_id) ||
            Common::IsNotNullOrEmptyString($params->crypto_addr);

        if ($cryptoInfoChanged) {
            $memberCryptoAccountRepository = new MemberCryptoAccountRepository();
            //將全部status改為0
            $memberCryptoAccountRepository->setDisableByMemberId($params->member_id);
            $result = $memberCryptoAccountRepository->insertOrUpdateData(
                $params->member_id,
                $params->crypto_id,
                $params->crypto_addr
            );
            if (!$result) {
                throw new Exception(__('error_message.2001'), 2001);
            }
        }

        return true;
    }

    public function setAvatar($member_id, $avatar_id)
    {
        $data = array();
        $data['avatar_id'] = $avatar_id;
        return $this->updateById($member_id, $data);
    }

    public function updateMemberIdCardId($member_id, $member_id_card_id)
    {
        $data = array();
        $data['member_id_card_id'] = $member_id_card_id;
        return $this->updateById($member_id, $data);
    }

    public function updateGoogle2FaSecretKey($member_id, $secret_key)
    {
        $data = array();
        $data['google2fa_secret'] = $secret_key;
        $data['is_2fa_enable'] = 0;
        return $this->updateById($member_id, $data);
    }

    public function set2faEnable($member_id, $is_2fa_enable)
    {
        if ($is_2fa_enable == 0) {
            $data = array();
            $data['is_2fa_enable'] = 1;
            return $this->updateById($member_id, $data);
        }
        return true;
    }

    public function checkMobileNumberExists($mobile)
    {
        $count = $this->repository->getCountByMobile($mobile);
        if ($count > 0) {
            throw new Exception(__('error_message.1102'), 1102);
        }
    }

    public function getSystemDetect($username, $agent_id)
    {
        $result = $this->repository->getSystemDetect($username, $agent_id);
        return $result;
    }

    public function getList($params)
    {
        $page = isset($params->page) ? $params->page : 1;
        $per_page = isset($params->per_page) ? $params->per_page : 50;
        $model = $this->repository->getList($params);
        $total = $model->count();
        $result = $model->orderBy('m.id', 'desc')
            ->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();

        $result = $result->map(function ($item) {
            $item->typeTxt = __('param.member.type.' . $item->type);
            return $item;
        });
        return [$total, $result];
    }

    public function getView($params) {}

    public function getWallet($member_id, $wallet_type_id = 0, $language_id = 1)
    {
        $memberWalletRepository = new MemberWalletRepository();
        $result = $memberWalletRepository->findByMemberId($member_id, $wallet_type_id, $language_id);
        return $result;
    }

    //type 撈取用戶帳號類型 0=全部; 1=銀行卡; 2=加密錢包;
    public function getBankAccount($member_id, $bank_id = 0, $type = 0)
    {
        $bank_account = [];
        $crypto_account = [];
        if (in_array($type, [0, 1])) {
            $memberBankAccountRepository = new MemberBankAccountRepository();
            $bank_account = $memberBankAccountRepository->getByMemberId($member_id, $bank_id);
        }

        if (in_array($type, [0, 2])) {
            $memberCryptoAccountRepository = new MemberCryptoAccountRepository();
            $crypto_account = $memberCryptoAccountRepository->getByMemberId($member_id, $bank_id);
        }
        return [$bank_account, $crypto_account];
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
        $result = $memberMinerRebateRepository->getSummaryList($member_id);
        return $result;
    }

    public function getRewards($member_id)
    {
        $memberMinerRewardRepository = new MemberMinerRewardRepository();
        $result = $memberMinerRewardRepository->getSummaryList($member_id);
        return $result;
    }

    public function getProfitAnalysisList($params)
    {
        $page = isset($params->page) ? $params->page : 1;
        $per_page = isset($params->per_page) ? $params->per_page : 50;
        $model = $this->repository->getProfitAnalysisList($params);
        $total = $model->count();
        $result = $model->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();
        $result = $result->map(function ($item) {
            $item->transactionTypeTxt =  __('param.transaction_type.' . $item->transaction_type_id);
            return $item;
        });
        return [$total, $result];
    }

    public function getAccountList($params)
    {
        $page = isset($params->page) ? $params->page : 1;
        $per_page = isset($params->per_page) ? $params->per_page : 50;
        $model = $this->repository->getAccountList($params);
        $total = $model->count();
        $result = $model->orderBy('m.id', 'desc')
            ->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();
        return [$total, $result];
    }

    public function getAgentCenter($params)
    {
        //取得玩家
        $member = $this->repository->findByUsername($params->username, $params->HIERARCHY_PATH);
        if (!is_object($member)) {
            throw new Exception(__('error_message.5007'), 5007);
        }

        //取得當前代理最大層級
        $max_level = ($this->repository->getMaxLevelById($member->id) - $member->level) + 1;

        $depositOrderRepository = new DepositOrderRepository();
        $withdrawalOrderRepository = new WithdrawalOrderRepository();
        $member_ids = array();
        $this_level_member_ids = array();
        $member_ids[] = $member->id;
        $invite_code = array();
        $invite_code[] = $member->invitation_code;
        $level_detail = array();
        $query_level = Common::IsNotNullOrEmptyString($params->level) ? $params->level : 0;
        $team_count = 0;
        $total_balance = $total_deposit = $total_withdrawal = 0;
        $total_deposit_member_count = $total_withdrawal_member_count = 0;

        for ($level = 1; $level < $max_level; $level++) {
            $level_count = 0;
            $deposit = $withdrawal = 0;
            $deposit_member_count = $withdrawal_member_count = 0;
            $this_level_member_ids = array();
            if (count($invite_code) > 0) {
                $teams = $this->repository->getTeams($invite_code);
                $invite_code = array();
                foreach ($teams as $row) {
                    $team_count++;
                    $level_count++;
                    if (($query_level == 0 || $level == $query_level)) {
                        $agent = array();
                        $agent['account'] = StringHelper::stringEncrypt($row->username ?? '', 2, 2, 6);
                        $agent['level'] = $level;
                        $agent['created_at'] = date('Y-m-d H:i:s', strtotime($row->created_at . '+8hour'));

                        $team[] = $agent;
                        $member_ids[] = $row->id;
                        $this_level_member_ids[] = $row->id;
                    }
                    $invite_code[] = $row->invitation_code;
                }

                //級別總充值、級別總提現"
                $this_deposit = $depositOrderRepository->getSummaryByMemberIds($this_level_member_ids);
                $this_withdrawal = $withdrawalOrderRepository->getSummaryByMemberIds($this_level_member_ids);

                $deposit = $this_deposit->point;
                $deposit_member_count = $this_deposit->member_count;
                $withdrawal = $this_withdrawal->point;
                $withdrawal_member_count = $this_withdrawal->member_count;
            }

            $level_detail[] = array(
                'level' => $level,
                'data' => array(
                    'count' => $level_count,
                    'deposit' => $deposit,
                    'withdrawal' => $withdrawal
                )
            );

            $total_deposit += $deposit;
            $total_deposit_member_count += $deposit_member_count;
            $total_withdrawal += $withdrawal;
            $total_withdrawal_member_count += $withdrawal_member_count;
        }

        $page = isset($params->page) ? $params->page : 1;
        $per_page = isset($params->per_page) ? $params->per_page : 50;
        $model = $this->repository->getAgentCenter($params, $member_ids, $member);
        $total = $model->count();
        $result = $model->skip(($page - 1) * $per_page)
            ->take($per_page)
            ->get();

        //总余额（下级账号里的所有钱包余额加上矿机残值）
        $total_balance = 0;

        $memberMinerRepository = new MemberMinerRepository();
        $result = $result->map(function ($item) use ($params, $memberMinerRepository, &$total_balance) {
            $total_balance += ($item->balance * 1);
            $item->created_at = Carbon::parse($item->created_at, 'UTC')->setTimezone('Asia/Taipei')->format('Y-m-d H:i:s');

            //miner
            $miner = $memberMinerRepository->getMyMinerSummary($params, $item->member_id);
            //餘額加上礦機殘值
            $item->residual_value = 0;
            foreach ($miner as $row) {
                $residual_value = $row->residual_value;
                $total_balance += $residual_value;
                $item->residual_value += $residual_value;
            }

            $item->miner = $miner;
            $item->minerCount = count($item->miner ?? []);

            return $item;
        });

        //总充值金额、总充值人数、总提现金额、总提现人数、总余额（下级账号里的所有钱包余额加上矿机残值）
        $summary = array();
        $summary['deposit'] = $total_deposit;  //总充值金额
        $summary['deposit_member_count'] = $total_deposit_member_count;  //总充值人数
        $summary['withdrawal'] = $total_withdrawal;  //总提现金额
        $summary['withdrawal_member_count'] = $total_withdrawal_member_count;  //总提现人数
        $summary['balance'] = $total_balance;  //总余额（下级账号里的所有钱包余额加上矿机残值）
        return [$total, $result, $summary, $level_detail, $max_level];
    }

    public function updateStatus($params)
    {
        return $this->updateById($params->member_id, ['status' => $params->status]);
    }

    public function updatePasswrodLock($params)
    {
        return $this->updateById($params->member_id, ['status' => $params->status, 'login_fail_count' => $params->failCount]);
    }

    public function resetGoogle2fa($params)
    {
        return $this->updateById($params->member_id, ['is_2fa_enable' => 0, 'google2fa_secret' => '']);
    }

    public function setFirstDeposit($member_id)
    {
        return $this->repository->setFirstDeposit($member_id);
    }

    public function setFirstWithdrawal($member_id)
    {
        return $this->repository->setFirstWithdrawal($member_id);
    }

    public function getWithdrawalData($params): array
    {
        $memberMainWallet = $this->getMainWallet($params->member_id);
        $memberBankAccountRepository = new MemberBankAccountRepository();
        $bank_account = $memberBankAccountRepository->getBankByMemberId($params->member_id, 0) ?? [];
        $memberCryptoAccountRepository = new MemberCryptoAccountRepository();
        $crypto_account = $memberCryptoAccountRepository->getCryptoByMemberId($params->member_id, 0) ?? [];
        return [
            'balance' => $memberMainWallet->balance ?? 0,
            WithdrawalEnum::Channel['bank'] => $bank_account,
            WithdrawalEnum::Channel['crypto'] => $crypto_account
        ];
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
