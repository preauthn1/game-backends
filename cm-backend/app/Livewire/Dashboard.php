<?php

namespace App\Livewire;

use App\Enums\PublicEnum;
use App\Http\Controllers\Dashboard\MainController;
use Livewire\Component;

class Dashboard extends Component
{
    public function mount()
    {
        //接口
        $common = new Common();
        $infoRequests = $common->getRequest([]);
        if (PublicEnum::ResponseCode['needLogin'] == $infoRequests['code']) {
            $common->doLogout();
        }
        $dashboardController = new MainController();
        $infoResult = $dashboardController->getInfo($infoRequests['request'])->getData();
        if (PublicEnum::ResponseCode['success'] == $infoResult->code) {
            $pageCacheData = [
                'total_registration' => $infoResult->data->total_registration ?? '-',
                'today_registration_number' => $infoResult->data->today_registration_number ?? '-',
                'today_depositor_number' => $infoResult->data->today_depositor_number ?? '-',
                'today_deposit_amount' => $infoResult->data->today_deposit_amount ?? '-',
                'today_withdrawal_amount' => $infoResult->data->today_withdrawal_amount ?? '-',
                'total_deposit_amount' => $infoResult->data->total_deposit_amount ?? '-',
                'total_deposit_number' => $infoResult->data->total_deposit_number ?? '-',
                'total_withdrawal_amount' => $infoResult->data->total_withdrawal_amount ?? '-',
                'total_withdrawal_number' => $infoResult->data->total_withdrawal_number ?? '-',
                'total_balance' => $infoResult->data->total_balance ?? '-',
                'showChart' => 1,
                'deposit_list_7' => collect($infoResult->data->deposit_list_7)->toArray(),
                'deposit_list_30' => collect($infoResult->data->deposit_list_30)->toArray(),
                'withdrawal_list_7' => collect($infoResult->data->withdrawal_list_7)->toArray(),
                'withdrawal_list_30' => collect($infoResult->data->withdrawal_list_30)->toArray(),
            ];
        } else {
            $pageCacheData = [
                'total_registration' => '-',
                'today_registration_number' => '-',
                'today_depositor_number' => '-',
                'today_deposit_amount' => '-',
                'today_withdrawal_amount' => '-',
                'total_deposit_amount' => '-',
                'total_deposit_number' => '-',
                'total_withdrawal_amount' => '-',
                'total_withdrawal_number' => '-',
                'total_balance' => '-',
                'showChart' => 0,
                'deposit_list_7' => [],
                'deposit_list_30' => [],
                'withdrawal_list_7' => [],
                'withdrawal_list_30' => [],
            ];
        }
        session()->put('pageCacheData', $pageCacheData);
    }

    public function render()
    {
        return view('livewire.dashboard');
    }
}
