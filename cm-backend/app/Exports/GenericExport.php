<?php

namespace App\Exports;

use App\Enums\PublicEnum;
use App\Http\Controllers\Record\TransactionLogController;
use App\Livewire\Common;
use Maatwebsite\Excel\Concerns\FromCollection;
use App\Http\Controllers\Member\MemberController;
use App\Http\Controllers\Report\ReportController;

class GenericExport implements FromCollection
{
    protected string $name;
    protected array $params;

    public function __construct($name, $params = [])
    {
        $this->name = $name;
        $this->params = $params;
    }

    public function collection()
    {
        $common = new Common();
        $requests = $common->getRequest($this->params ?? []);
        if (PublicEnum::ResponseCode['needLogin'] == $requests['code']) {
            $common->doLogout();
        }
        $fields = [];
        $data = [];
        $result = [];
        switch ($this->name) {
            case 'memberList':
                $memberController = new MemberController();
                $memberList = $memberController->getList($requests['request'])->getData();
                $data = $memberList->data->list ?? [];
                if (!empty($data)) {
                    $fields[] = collect($data[0])->keys()->all();
                }
                $result = array_merge($fields, $data);
                break;
            case 'intervalDailyReport':
                $reportController = new ReportController();
                $reportList = $reportController->getReport($requests['request'])->getData();
                $data = $reportList->data->data ?? [];
                if (!empty($data)) {
                    $title = collect($data[0])->keys()->all();
                    foreach ($title as $name) {
                        $dataFields[0][] = __('web_message.intervalDailyReport.' . $name);
                    }
                }
                $statistic = $reportList->data->statistic ?? [];
                if (!empty($statistic->rangeTotal ?? [])) {
                    if (count($statisticFields[0] ?? []) < 1) {
                        $title = collect($statistic->rangeTotal ?? [])->keys()->all();
                        foreach ($title as $name) {
                            $statisticFields[0][] = __('web_message.intervalDailyReport.' . $name);
                        }
                    }
                    $statisticData['rangeTotal'] = collect($statistic->rangeTotal ?? [])->toArray();
                    $statisticData['rangeTotal']['type'] = __('web_message.intervalDailyReport.' . $statisticData['rangeTotal']['type']);
                }
                if (!empty($statistic->dayAverage ?? [])) {
                    if (count($statisticFields[0] ?? []) < 1) {
                        $title = collect($statistic->dayAverage ?? [])->keys()->all();
                        foreach ($title as $name) {
                            $statisticFields[0][] = __('web_message.intervalDailyReport.' . $name);
                        }
                    }
                    $statisticData['dayAverage'] = collect($statistic->dayAverage ?? [])->toArray();
                    $statisticData['dayAverage']['type'] = __('web_message.intervalDailyReport.' . $statisticData['dayAverage']['type']);
                }
                if (!empty($statistic->maxValue ?? [])) {
                    if (count($statisticFields[0] ?? []) < 1) {
                        $title = collect($statistic->maxValue ?? [])->keys()->all();
                        foreach ($title as $name) {
                            $statisticFields[0][] = __('web_message.intervalDailyReport.' . $name);
                        }
                    }
                    $statisticData['maxValue'] = collect($statistic->maxValue ?? [])->toArray();
                    $statisticData['maxValue']['type'] = __('web_message.intervalDailyReport.' . $statisticData['maxValue']['type']);
                }
                if (!empty($statistic->minValue ?? [])) {
                    if (count($statisticFields[0] ?? []) < 1) {
                        $title = collect($statistic->minValue ?? [])->keys()->all();
                        foreach ($title as $name) {
                            $statisticFields[0][] = __('web_message.intervalDailyReport.' . $name);
                        }
                    }
                    $statisticData['minValue'] = collect($statistic->minValue ?? [])->toArray();
                    $statisticData['minValue']['type'] = __('web_message.intervalDailyReport.' . $statisticData['minValue']['type']);
                }
                $result = array_merge($statisticFields ?? [], $statisticData ?? [], $dataFields ?? [], $data);
                break;
            case 'manualOperation':
                $transactionLogController = new TransactionLogController();
                $dataList = $transactionLogController->getManualExport($requests['request'])->getData();
                $data = $dataList->data->list ?? [];
                if (!empty($data)) {
                    $title = collect($data[0])->keys()->all();
                    foreach ($title as $name) {
                        $dataFields[0][] = __('web_message.general.' . $name);
                    }
                }
                $result = array_merge($dataFields ?? [], $data);
                break;
        }
        return collect($result);
    }
}
