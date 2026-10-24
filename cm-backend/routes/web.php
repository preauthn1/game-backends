<?php

use Illuminate\Support\Facades\Route;
use Illuminate\Support\Facades\File;
use App\Helpers\Common;

Route::get('/', Login::class)->name('login');
Route::get('/login', Login::class);

Route::get('/infos', function () {
    $path = base_path('version.json');
    $ip = Common::getUserIp() ?? '';
    $approvedIps = '0.0.0.0,0.0.0.0';
    $result = ['ip' => $ip];
    if (File::exists($path) && (in_array($ip, explode(',', $approvedIps)))) {
        $json = File::get($path);
        $data = json_decode($json, true);
        if (!$data) $data = ['message' => json_last_error_msg()];
        $result = array_merge($result, $data);
    }
    return response()->json($result);
});

Route::middleware(['auth_web'])->group(function () {
    /** Dashboard */
    Route::get('/dashboard', Dashboard::class);
    /** System */
    Route::get('/menuMgmt', System\MenuMgmt::class);
    Route::get('/groupDefine', System\GroupDefine::class);
    Route::get('/depositSetting', System\DepositSetting::class);
    Route::get('/depositAccountSetting', System\DepositAccountSetting::class);
    Route::get('/withdrawalSetting', System\WithdrawalSetting::class);
    Route::get('/mlm', System\MLM::class);
    Route::get('/invitePromotion', System\InvitePromotion::class);
    Route::get('/promotionSetting', System\PromotionSetting::class);
    Route::get('/siteMessage', System\SiteMessage::class);
    Route::get('/announcement', System\Announcement::class);
    Route::get('/downloadPage', System\DownloadPage::class);
    Route::get('/rankingList', System\RankingList::class);
    Route::get('/minerSetting', System\MinerSetting::class);
    Route::get('/groupOrderSetting', System\GroupOrderSetting::class);
    Route::get('/miningBlockSetting', System\MiningBlockSetting::class);
    Route::get('/countryCodeMgmt', System\CountryCodeMgmt::class);
    Route::get('/taskScheduling', System\TaskScheduling::class);
    /** Member */
    Route::get('/memberList', Member\MemberList::class);
    Route::get('/memberDetail', Member\MemberDetail::class);
    Route::get('/idInformation', Member\IdInformation::class);
    Route::get('/profitAnalysis', Member\ProfitAnalysis::class);
    Route::get('/manualOperation', Member\ManualOperation::class);
    /** Agent */
    Route::get('/agentDetail', Agent\AgentDetail::class);
    /** Deposit */
    Route::get('/depositList', Deposit\DepositList::class);
    Route::get('/depositChannel', Deposit\DepositChannel::class);
    /** Withdrawal */
    Route::get('/withdrawalList', Withdrawal\WithdrawalList::class);
    Route::get('/withdrawalChannel', Withdrawal\WithdrawalChannel::class);
    /** FinancialMgmt */
    Route::get('/purchaseList', FinancialMgmt\PurchaseList::class);
    Route::get('/rentList', FinancialMgmt\RentList::class);
    Route::get('/groupList', FinancialMgmt\GroupList::class);
    Route::get('/depreciationInfo', FinancialMgmt\DepreciationInfo::class);
    Route::get('/rebateInfo', FinancialMgmt\RebateInfo::class);
    Route::get('/inviteRewardInfo', FinancialMgmt\InviteRewardInfo::class);
    /** Manager */
    Route::get('/roleMgmt', Manager\RoleMgmt::class);
    Route::get('/managerList', Manager\ManagerList::class);
    Route::get('/operationLog', Manager\OperationLog::class);
    Route::get('/groupMgmt', Manager\GroupMgmt::class);
    /** Report */
    Route::get('/intervalDailyReport', Report\IntervalDailyReport::class);
});
