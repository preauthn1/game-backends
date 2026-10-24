<?php

namespace App\Repositories;

use Illuminate\Support\Carbon;
use Illuminate\Support\Str;
use JasonGuru\LaravelMakeRepository\Repository\BaseRepository;
use App\Models\Member;
use App\Helpers\Common;
use App\Enums\TimeEnum;
use DB;

/**
 * Class MemberRepository.
 */
class MemberRepository extends BaseRepository
{
    /**
     * @return string
     *  Return the model
     */
    public function model()
    {
        return Member::class;
    }

    public function find($id)
    {
        return $this->model->find($id);
    }

    public function filter($array, array $columns = ['*'])
    {
        return $this->model->where($array)
            ->get($columns);
    }

    public function findByUsername($username, $hierarchy_path = '')
    {
        return $this->model
            ->join('agents as a', function ($join) use ($hierarchy_path) {
                $join->on('a.id', 'members.agent_id')
                    ->when($hierarchy_path != '', function ($query) use ($hierarchy_path) {
                        $query->where('a.hierarchy_path', 'like', $hierarchy_path . '%');
                    });
            })
            ->where('members.username', $username)
            ->where('members.status', 1)
            ->select('members.*')
            ->first();
    }

    public function findByMobile($mobile)
    {
        return $this->model->where('mobile', $mobile)
            ->where('mobile_validate', 1)
            ->first();
    }

    public function getCountByMobile($mobile)
    {
        return $this->model->where('mobile', $mobile)
            ->where('members.mobile_validate', 1)
            ->count();
    }

    public function setFirstDeposit($member_id)
    {
        return $this->model->where('id', $member_id)
            ->whereNull('first_deposit')
            ->update(['first_deposit' => date('Y-m-d H:i:s')]);
    }

    public function setFirstWithdrawal($member_id)
    {
        return $this->model->where('id', $member_id)
            ->whereNull('first_withdrawal')
            ->update(['first_withdrawal' => date('Y-m-d H:i:s')]);
    }

    public function getSystemDetect($username, $agent_id)
    {
        return $this->model->where('username', 'like', $username . '_%')
            ->where('agent_id', $agent_id)
            ->first();
    }

    // 取得最下級代理階層level
    public function getMaxLevelById($member_ids)
    {
        $ids = DB::table('members as p')
            ->join('members as m', 'm.referral_code', 'p.invitation_code')
            ->when(in_array(gettype($member_ids), ['integer', 'string']), function ($query) use ($member_ids) {
                $query->where('p.id', $member_ids);
            })
            ->when(gettype($member_ids) == 'array', function ($query) use ($member_ids) {
                $query->whereIn('p.id', $member_ids);
            })
            ->pluck('m.id')
            ->toArray();
        return (count($ids) > 0)
            ? $this->getMaxLevelById($ids)
            : $this->model
            ->when(in_array(gettype($member_ids), ['integer', 'string']), function ($query) use ($member_ids) {
                $query->where('id', $member_ids);
            })
            ->when(gettype($member_ids) == 'array', function ($query) use ($member_ids) {
                $query->whereIn('id', $member_ids);
            })
            ->select(DB::raw('max(level) as level'))->first()->level;
    }

    /**
     * 獲取不重複的推薦碼
     */
    public function getInvitationCode(): string
    {
        $length = 6;
        $randomString = '23456789ABCDEFGHJKMN23456789PQRSTUVWXYZ23456789ABCDEFGHJKMN23456789PQRSTUVWXYZ';
        $isDuplicate = false;
        do {
            $start = mt_rand(1, 70);
            $code = substr(str_shuffle($randomString), $start, $length);
            $sameCodeMember = $this->model->where('invitation_code', $code)->first();
            if (!is_object($sameCodeMember)) {
                $isDuplicate = true;
            }
        } while (!$isDuplicate);
        return $code;
    }

    /**
     * 獲取不重複的UID
     */
    public function getUid(): int
    {
        do {
            $uid = rand(10000, 999999);
            $sameCodeMember = $this->model->where('uid', $uid)->first();
            if (!is_object($sameCodeMember)) {
                $isDuplicate = true;
            }
        } while (!$isDuplicate);
        return $uid;
    }

    public function getList($params)
    {
        $subMba = DB::table('member_bank_accounts as mba')
            ->leftJoin('banks as bba', function ($join) {
                $join->on('mba.bank_id', 'bba.id')
                    ->where('bba.status', 1);
            })
            ->leftJoin('currencies as cba', function ($join) {
                $join->on('bba.currency_id', 'cba.id')
                    ->where('cba.status', 1);
            })
            ->where('mba.status', 1)
            ->where('cba.support_withdrawal', 1)
            ->select(
                'mba.id',
                'mba.member_id',
                'mba.bank_id',
                'mba.account_name',
                'mba.account_no'
            );
        $subMca = DB::table('member_crypto_accounts as mca')
            ->leftJoin('banks as bca', function ($join) {
                $join->on('mca.bank_id', 'bca.id')
                    ->where('bca.status', 1);
            })
            ->leftJoin('currencies as cca', function ($join) {
                $join->on('bca.currency_id', 'cca.id')
                    ->where('cca.status', 1);
            })
            ->where('mca.status', 1)
            ->where('cca.support_withdrawal', 1)
            ->select(
                'mca.id',
                'mca.member_id',
                'mca.bank_id',
                'mca.address',
            );
        $subIpCount = DB::table('members as ipc')
            ->groupBy('ipc.last_login_ip')
            ->select(
                'ipc.last_login_ip as ip',
                DB::raw('COUNT(ipc.last_login_ip) as lastLoginIp_count')
            );
        $subMobileCount = DB::table('members as mc')
            ->groupBy('mc.mobile')
            ->select(
                'mc.mobile as mobile',
                DB::raw('COUNT(mc.mobile) as mobile_count')
            );
        //帳號、UID、上級（各級別至最高上級）、姓名、手機號、邀請碼、餘額、
        //註冊IP、註冊時間、最後登入IP、最後登入時間
        return DB::table('members as m')
            ->join('agents as a', function ($join) use ($params) {
                $join->on('a.id', 'm.agent_id')
                    ->when(Common::IsNotNullOrEmptyString($params->HIERARCHY_PATH), function ($query) use ($params) {
                        $query->where('a.hierarchy_path', 'like', $params->HIERARCHY_PATH . '%');
                    });
            })
            ->leftJoin('members as pm', 'pm.invitation_code', 'm.referral_code')
            ->leftJoin('member_wallets as mw', function ($join) {
                $join->on('mw.member_id', 'm.id')
                    ->where('mw.wallet_type_id', 1);
            })
            ->leftJoin(DB::raw('(' . $subMba->toSql() . ') as mba'), function ($join) {
                $join->on('mba.member_id', 'm.id');
            })
            ->mergeBindings($subMba)
            ->leftJoin(DB::raw('(' . $subMca->toSql() . ') as mca'), function ($join) {
                $join->on('mca.member_id', 'm.id');
            })
            ->mergeBindings($subMca)
            ->leftJoin('countries as cts', function ($join) use ($params) {
                $join->on('cts.id', 'm.country_id')
                    ->where('cts.status', 1);
            })
            ->leftJoin(DB::raw('(' . $subIpCount->toSql() . ') as ipc'), function ($join) {
                $join->on('ipc.ip', 'm.last_login_ip');
            })
            ->leftJoin(DB::raw('(' . $subMobileCount->toSql() . ') as mc'), function ($join) {
                $join->on('mc.mobile', 'm.mobile');
            })
            ->when(Common::IsNotNullOrEmptyString($params->uid), function ($query) use ($params) {
                $query->where('m.uid', $params->uid);
            })
            ->when(Common::IsNotNullOrEmptyString($params->type), function ($query) use ($params) {
                $query->where('m.type', $params->type);
            })
            ->when(Common::IsNotNullOrEmptyString($params->username), function ($query) use ($params) {
                if ($params->fuzzy_query == 1) {
                    $query->where('m.username', 'like', '%' . $params->username . '%');
                } else {
                    $query->where('m.username', $params->username);
                }
            })
            ->when(Common::IsNotNullOrEmptyString($params->parent_username), function ($query) use ($params) {
                if ($params->fuzzy_query == 1) {
                    $query->where('pm.username', 'like', '%' . $params->parent_username . '%');
                } else {
                    $query->where('pm.username', $params->parent_username);
                }
            })
            ->when(Common::IsNotNullOrEmptyString($params->real_name), function ($query) use ($params) {
                $query->where('m.real_name', $params->real_name);
            })
            ->when(Common::IsNotNullOrEmptyString($params->country_id), function ($query) use ($params) {
                $query->where('m.country_id', $params->country_id);
            })
            ->when(Common::IsNotNullOrEmptyString($params->mobile), function ($query) use ($params) {
                $query->where('m.mobile', $params->mobile);
            })
            ->when(Common::IsNotNullOrEmptyString($params->invite_code), function ($query) use ($params) {
                $query->where('m.invitation_code', $params->invite_code);
            })
            ->when(Common::IsNotNullOrEmptyString($params->register_ip), function ($query) use ($params) {
                $query->where('m.register_ip', ip2long($params->register_ip));
            })
            ->when(Common::IsNotNullOrEmptyString($params->login_ip), function ($query) use ($params) {
                $query->where('m.last_login_ip', ip2long($params->login_ip));
            })
            ->when(Common::IsNotNullOrEmptyString($params->start_time), function ($query) use ($params) {
                $start_time = date('Y-m-d H:i:00', strtotime($params->start_time . '-8hour'));
                $query->where('m.created_at', '>=', $start_time);
            })
            ->when(Common::IsNotNullOrEmptyString($params->end_time), function ($query) use ($params) {
                $end_time = date('Y-m-d H:i:59', strtotime($params->end_time . '-8hour'));
                $query->where('m.created_at', '<=', $end_time);
            })
            ->when(Common::IsNotNullOrEmptyString($params->account_no), function ($query) use ($params) {
                $query->where('mba.account_no', $params->account_no);
            })
            ->when(Common::IsNotNullOrEmptyString($params->crypto_addr), function ($query) use ($params) {
                $query->where('mca.address', $params->crypto_addr);
            })
            ->when(Common::IsNotNullOrEmptyString($params->status), function ($query) use ($params) {
                $query->where('m.status', $params->status);
            })
            ->where('m.type', '!=', 9)
            ->select(
                'm.id as id',
                'm.uid as uid',
                'm.username as username',
                'a.username as agent_username',
                'pm.username as parent_username',
                'm.real_name as real_name',
                'm.nickname as nickname',
                'm.type',
                'm.country_id as country_id',
                'cts.country_code as country_code',
                'm.mobile as mobile',
                'm.invitation_code as invite_code',
                DB::raw('IFNULL(mw.balance, 0) as balance'),
                DB::raw('INET_NTOA(m.register_ip) as register_ip'),
                DB::raw('DATE_ADD(m.created_at, INTERVAL 8 HOUR) as register_time'),
                DB::raw('INET_NTOA(m.last_login_ip) as last_login_ip'),
                DB::raw('DATE_ADD(m.last_login, INTERVAL 8 HOUR) as last_login_time'),
                'mba.id as account_bank_id',
                'mba.account_name',
                'mba.account_no',
                'mca.id as account_crypto_id',
                'mca.address as crypto_addr',
                'm.memo',
                'ipc.lastLoginIp_count',
                'mc.mobile_count',
                'm.status'
            );
    }

    public function getProfitAnalysisList($params)
    {
        //"總盈利=餘額+總提現金額-總充值金額
        //會員帳號、總充值金額、總提現金額、總購買金額、
        //總銷售返佣、總邀請獎勵、總兌換USDT金額、總兌換COIN、
        //總折舊金額、人工增加、人工減少、總盈利
        $start_time = '';
        if (Common::IsNotNullOrEmptyString($params->start_time)) {
            $start_time = date('Y-m-d H:i:00', strtotime($params->start_time . '-8hour'));
        }
        $end_time = '';
        if (Common::IsNotNullOrEmptyString($params->end_time)) {
            $end_time = date('Y-m-d H:i:59', strtotime($params->end_time . '-8hour'));
        }

        $depreciationModel = DB::table('member_miners as mm')
            ->join('member_miner_depreciations as mmd', function ($join) use ($start_time, $end_time) {
                $join->on('mmd.member_miner_id', 'mm.id')
                    ->whereRaw("mmd.summary_time BETWEEN '" . $start_time . "' AND '" . $end_time . "'");
            })
            ->groupBy('mm.member_id')
            ->select(
                'mm.member_id',
                DB::raw('IFNULL(SUM(mmd.depreciation_amount), 0) AS depreciation_amount')
            );

        $transactionModel = DB::table('members as m')
            ->join('agents as a', function ($join) use ($params) {
                $join->on('a.id', 'm.agent_id')
                    ->where('a.hierarchy_path', 'like', $params->HIERARCHY_PATH . '%');
            })
            ->join('transaction_logs as tl', function ($join) use ($start_time, $end_time) {
                $join->on('tl.member_id', 'm.id')
                    ->whereRaw("tl.created_at BETWEEN '" . $start_time . "' AND '" . $end_time . "'");
            })
            ->when(Common::IsNotNullOrEmptyString($params->username), function ($query) use ($params) {
                $query->where('m.username', $params->username);
            })
            ->when(Common::IsNotNullOrEmptyString($params->uid), function ($query) use ($params) {
                $query->where('m.uid', $params->uid);
            })
            ->groupBy('m.id')
            ->select(
                DB::raw('min(tl.id) AS transaction_log_min_id'),
                DB::raw('max(tl.id) AS transaction_log_max_id'),
                'm.id as member_id',
                'tl.transaction_type_id',
                //總充值金額
                DB::raw('SUM(CASE WHEN tl.transaction_type_id = 1 THEN tl.point ELSE 0 END) AS deposit_amount'),
                //總提現金額
                DB::raw('SUM(CASE WHEN tl.transaction_type_id = 2 THEN tl.point ELSE 0 END) AS withdrawal_amount'),
                //總購買金額
                DB::raw('SUM(CASE WHEN tl.transaction_type_id = 3 THEN tl.point ELSE 0 END) AS purchase_amount'),
                DB::raw('SUM(CASE WHEN tl.transaction_type_id = 4 THEN tl.point ELSE 0 END) AS rental_amount'),
                //挖礦利潤
                DB::raw('SUM(CASE WHEN tl.transaction_type_id = 7 THEN tl.point ELSE 0 END) AS mining_profits'),
                //挖礦利潤(團購)
                DB::raw('SUM(CASE WHEN tl.transaction_type_id = 8 THEN tl.point ELSE 0 END) AS group_mining_profits'),
                //挖礦利潤(租賃)
                DB::raw('SUM(CASE WHEN tl.transaction_type_id = 9 THEN tl.point ELSE 0 END) AS rent_mining_profits'),
                //總銷售返佣
                DB::raw('SUM(CASE WHEN tl.transaction_type_id = 11 THEN tl.point ELSE 0 END) AS rebate_amount'),
                //總邀請獎勵
                DB::raw('SUM(CASE WHEN tl.transaction_type_id = 10 THEN tl.point ELSE 0 END) AS reward_amount'),
                //人工增加
                DB::raw('SUM(CASE WHEN tl.transaction_type_id = 12 THEN tl.point ELSE 0 END) AS manual_add'),
                //人工減少
                DB::raw('SUM(CASE WHEN tl.transaction_type_id = 13 THEN tl.point ELSE 0 END) AS manual_sub'),
                //人工獎勵
                DB::raw('SUM(CASE WHEN tl.transaction_type_id = 16 THEN tl.point ELSE 0 END) AS manual_reward'),
                //人工扣分
                DB::raw('SUM(CASE WHEN tl.transaction_type_id = 17 THEN tl.point ELSE 0 END) AS manual_deduct'),
                //體驗返佣
                DB::raw('SUM(CASE WHEN tl.transaction_type_id = 18 THEN tl.point ELSE 0 END) AS experience_commission'),
            );

        //主查詢
        $model = DB::table('members as m')
            ->join('agents as a', function ($join) use ($params) {
                $join->on('a.id', 'm.agent_id')
                    ->where('a.hierarchy_path', 'like', $params->HIERARCHY_PATH . '%');
            })
            //->join('member_wallets as mw', function ($join) {
            //    $join->on('mw.member_id', 'm.id')
            //        ->where('mw.wallet_type_id', 1);
            //})
            ->join(DB::raw('(' . $transactionModel->toSql() . ') as tl'), function ($join) {
                $join->on('tl.member_id', 'm.id');
            })
            ->join('transaction_logs AS be', 'be.id', 'tl.transaction_log_min_id')
            ->join('transaction_logs AS af', 'af.id', 'tl.transaction_log_max_id')
            ->mergeBindings($transactionModel)
            ->leftJoin(DB::raw('(' . $depreciationModel->toSql() . ') as d'), function ($join) {
                $join->on('d.member_id', 'm.id');
            })
            ->mergeBindings($depreciationModel)
            ->when(Common::IsNotNullOrEmptyString($params->username), function ($query) use ($params) {
                $query->where('m.username', $params->username);
            })
            ->when(Common::IsNotNullOrEmptyString($params->uid), function ($query) use ($params) {
                $query->where('m.uid', $params->uid);
            })
            ->when(Common::IsNotNullOrEmptyString($params->order_by), function ($query) use ($params) {
                $sort_column = $this->getAnalyzeSortColumn($params->order_by);
                $sort_by = (Common::IsNotNullOrEmptyStringNotZeroValue($params->sort) && $params->sort == 1)
                    ? 'ASC' : 'DESC';
                $query->orderBy($sort_column, $sort_by);
            })
            ->select(
                'm.uid',
                'm.id as member_id',
                'm.username',
                DB::raw('DATE_ADD(m.created_at, INTERVAL 8 HOUR) as created_at'),
                'tl.transaction_type_id',
                'be.before_balance AS before_balance',
                'af.after_balance AS after_balance',
                //盈餘 = 期末餘額 - 期初餘額 + 提現金額 - 充值金額
                DB::raw('(af.after_balance - be.before_balance + tl.withdrawal_amount - tl.deposit_amount) AS total_profit'),  //總盈利
                DB::raw('IFNULL(tl.deposit_amount, 0) AS deposit_amount'), //總充值金額
                DB::raw('IFNULL(tl.withdrawal_amount, 0) AS withdrawal_amount'), //總提現金額
                DB::raw('(tl.purchase_amount + tl.rental_amount) AS purchase_amount'), //總購買金額
                DB::raw('(tl.mining_profits + tl.group_mining_profits + tl.rent_mining_profits) AS mining_profits'),  //总挖矿收益
                DB::raw('IFNULL(d.depreciation_amount, 0) AS depreciation_amount'), //總折舊金額
                DB::raw('IFNULL(tl.rebate_amount, 0) AS rebate_amount'), //總銷售返佣
                DB::raw('IFNULL(tl.reward_amount, 0) AS reward_amount'), //總邀請獎勵
                DB::raw('IFNULL(tl.manual_add, 0) AS manual_add'), //人工上分
                DB::raw('IFNULL(tl.manual_sub, 0) AS manual_sub'), //人工下分
                DB::raw('IFNULL(tl.manual_reward, 0) AS manual_reward'), //人工獎勵
                DB::raw('IFNULL(tl.manual_deduct, 0) AS manual_deduct'), //人工扣分
                DB::raw('IFNULL(tl.experience_commission, 0) AS experience_commission'), //體驗返佣
            );
        return $model;
    }

    public function getAccountList($params)
    {
        //輸入篩選：會員帳號、會員姓名、銀行卡、USDT地址
        return DB::table('members as m')
            ->join('agents as a', function ($join) use ($params) {
                $join->on('a.id', 'm.agent_id')
                    ->where('a.hierarchy_path', 'like', $params->HIERARCHY_PATH . '%');
            })
            //->join('member_wallets as mw', 'mw.member_id', 'm.id')
            ->leftJoin('member_bank_accounts as mba', function ($join) use ($params) {
                $join->on('mba.member_id', 'm.id')
                    ->where('mba.status', 1)
                    ->when(isset($params->account_no), function ($query) use ($params) {
                        $query->where('mba.account_no', $params->account_no);
                    });
            })
            ->leftJoin('member_crypto_accounts as mca', function ($join) use ($params) {
                $join->on('mca.member_id', 'm.id')
                    ->where('mca.status', 1)
                    ->when(isset($params->crypto_addr), function ($query) use ($params) {
                        $query->where('mca.address', $params->crypto_addr);
                    });
            })
            ->when(isset($params->username), function ($query) use ($params) {
                $query->where('m.username', $params->username);
            })
            ->when(isset($params->real_name), function ($query) use ($params) {
                $query->where('m.real_name', $params->real_name);
            })
            ->select(
                'm.id as member_id',
                'm.uid',
                'm.username',
                'm.real_name',
                'mba.id as account_bank_id',
                'mba.account_name',
                'mba.account_no',
                'mca.id as account_crypto_id',
                'mca.address as crypto_addr'
            );
    }

    public function getTeams($invite_code)
    {
        return $this->model->whereIn('referral_code', $invite_code)
            ->where('status', 1)
            ->select(
                'id',
                'username',
                'invitation_code',
                'created_at'
            )
            ->get();
    }

    public function getAgentCenter($params, $member_ids, $member)
    {
        /**
         * 查看代理下級各級明細（含帳號、UID、餘額、註冊時間、已購買礦機型號及數量、金額）
         * 、總（各級）充值提現金額
         * "根據輸入篩選：帳號
         * 下拉篩選：下級級數（不選擇則默認所有下級）、
         * 礦機機型（不選擇默認下級個人所有購買金額）
         * 級別總人數、級別總充值、級別總提現"
         * */
        return DB::table('members as m')
            ->join('member_wallets as mw', 'mw.member_id', 'm.id')
            ->when(Common::IsNotNullOrEmptyString($params->name), function ($query) use ($params) {
                $sub = DB::table('miners')
                    ->join('member_miners as mm', function ($join) {
                        $join->on('mm.miner_id', 'miners.id')
                            ->whereIn('mm.type', [1, 2])
                            ->whereNotNull('mm.start_time');
                    })
                    ->join('member_miner_transaction_logs as mmtl', 'mm.id', 'mmtl.member_miner_id')
                    ->where('miners.name', $params->name)
                    ->groupBy('mm.member_id')
                    ->select('mm.member_id');
                $query->join(
                    DB::raw('(' . $sub->toRawSql() . ') as mm'),
                    'mm.member_id',
                    'm.id'
                );
            })
            ->leftJoin('members as p', 'p.invitation_code', 'm.referral_code')
            ->whereIn('m.id', $member_ids)
            ->when(Common::IsNotNullOrEmptyString($params->level), function ($query) use ($params, $member) {
                $query->where('m.level', ($params->level + $member->level));
            })
            ->orderBy('m.level', 'ASC')
            ->select(
                'm.id as member_id',
                'm.username',
                'm.uid',
                DB::raw('(m.level - ' . $member->level . ') as level'),
                'm.created_at',
                'mw.balance',
                DB::raw('IFNULL(p.id, 0) as parent_id'),
                DB::raw("IFNULL(p.username, '') as parent_username")
            );
    }

    public function getMemberCount($params)
    {
        //礦機餘額子查詢
        $subDepreciationModel = DB::table('member_miner_depreciations')
            ->groupBy('member_miner_id')
            ->select(
                'member_miner_id',
                DB::raw('max(id) as max_id')
            );
        $depreciationModel = DB::table('member_miner_depreciations as d')
            ->join(
                DB::raw('(' . $subDepreciationModel->toRawSql() . ') as ds'),
                'ds.max_id',
                'd.id'
            )
            ->select(
                'd.member_miner_id',
                'd.residual_value'
            );

        $memberMinerModel = DB::table('member_miners as mm')
            ->join('member_miner_transaction_logs as mmtl', 'mm.id', 'mmtl.member_miner_id')
            ->join('miners as miner', function ($join) {
                $join->on('mm.miner_id', 'miner.id')
                    ->whereIn('mm.type', [1, 2]);
            })
            ->leftJoin(
                DB::raw('(' . $depreciationModel->toRawSql() . ') as mmd'),
                'mmd.member_miner_id',
                'mm.id'
            )
            ->whereNotNull('mm.start_time')
            ->whereIn('mm.status', [1, 3])
            ->groupBy('mm.member_id')
            ->select(
                'mm.member_id',
                DB::raw('SUM(IFNULL(mmd.residual_value, case when mm.type = 1 then miner.purchase_price else miner.group_price end)) AS residual_value')
            );

        return DB::table('members as m')
            ->join('agents as a', function ($join) use ($params) {
                $join->on('a.id', 'm.agent_id')
                    ->where('a.hierarchy_path', 'like', $params->HIERARCHY_PATH . '%');
            })
            ->leftJoin('member_wallets as mw', function ($join) {
                $join->on('mw.member_id', 'm.id')
                    ->where('mw.wallet_type_id', 1);
            })
            ->leftJoin(
                DB::raw('(' . $memberMinerModel->toRawSql() . ') as mm'),
                'mm.member_id',
                'm.id'
            )
            ->where('m.type', 1)
            ->where('m.status', 1)
            ->select(
                DB::raw('COUNT(*) AS registration_count'),
                DB::raw('SUM(CASE WHEN first_deposit IS NOT NULL THEN 1 ELSE 0 END) AS deposit_count'),
                DB::raw('SUM(CASE WHEN first_withdrawal IS NOT NULL THEN 1 ELSE 0 END) AS withdrawal_count'),
                DB::raw('SUM(mw.balance) AS balance'),
                DB::raw('SUM(mm.residual_value) AS residual_value')
            )
            ->first();
    }

    public function getTodayRegistrationCount($params)
    {
        $todayStartMy = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::now()->startOfDay(), TimeEnum::TimeZone['backend']);
        $todayEndMy = Carbon::createFromFormat('Y-m-d H:i:s', Carbon::now()->endOfDay(), TimeEnum::TimeZone['backend']);
        $startTimeUTC = Carbon::parse($todayStartMy)->setTimezone(TimeEnum::TimeZone['db'])->format('Y-m-d H:i:s');
        $endTimeUTC = Carbon::parse($todayEndMy)->setTimezone(TimeEnum::TimeZone['db'])->format('Y-m-d H:i:s');

        return DB::table('members as m')
            ->join('agents as a', function ($join) use ($params) {
                $join->on('a.id', 'm.agent_id')
                    ->where('a.hierarchy_path', 'like', $params->HIERARCHY_PATH . '%');
            })
            ->where('m.created_at', '>=', $startTimeUTC)
            ->where('m.created_at', '<=', $endTimeUTC)
            ->where('m.type', 1)
            ->where('m.status', 1)
            ->count();
    }

    private function getAnalyzeSortColumn($column_id)
    {
        switch ($column_id) {
            case 1:
                return 'deposit_amount'; //總充值金額
                break;
            case 2:
                return 'withdrawal_amount'; //總提現金額
                break;
            case 3:
                return 'purchase_amount'; //總購買金額
                break;
            case 4:
                return 'rebate_amount'; //總銷售返佣
                break;
            case 5:
                return 'reward_amount'; //總邀請獎勵
                break;
            case 6:
                return 'mining_profits'; //总挖矿收益
                break;
            case 7:
                return 'depreciation_amount'; //總折舊金額
                break;
            case 8:
                return 'total_profit'; //總盈利
                break;
        }
    }
}
