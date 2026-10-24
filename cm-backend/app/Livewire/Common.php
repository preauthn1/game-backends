<?php

namespace App\Livewire;

use App\Enums\PublicEnum;
use App\Enums\TimeEnum as TimeEnum;
use Illuminate\Support\Carbon;
use Illuminate\Support\Facades\App;
use Livewire\Component;
use Illuminate\Http\Request;
use App\Http\Controllers\Auth\MainController;

class Common extends Component
{
    //修改語言
    public function changeLanguage($languageCode)
    {
        $language = PublicEnum::Language[$languageCode]['code'] ?? 'en';
        session()->put('language', $language);
        App::setLocale($language);
    }

    //登出
    public function doLogout()
    {
        session()->forget('userData');
        return redirect()->route('login');
    }

    //串接request
    public function getRequest($params)
    {
        $request = new Request();
        if (empty(session()->get('userData')->token ?? '')) {
            return ['code' => PublicEnum::ResponseCode['needLogin'], 'request' => $request];
        }
        //用token取出些基本參數
        $requests = $request->merge(['token' => session()->get('userData')->token]);
        $mainController = new MainController();
        $mainList = $mainController->Authenticate($requests)->getData();
        $basic = collect($mainList->data ?? [])->toArray() ?? [];
        //合併傳參
        $allParams = array_merge($basic, $params);
        return [
            'code' => $mainList->code,
            'request' => $request->merge($allParams),
        ];
    }

    //日期區間驗證
    public function checkDateRange($range = ''): array
    {
        $result = [
            'code' => 0,
            'message' => __('web_message.general.timeRange') . ' ' . __('web_message.general.error'),
            'range' => [],
        ];
        if (empty($range)) {
            return $result;
        }
        $rangeArray = explode(' ~ ', $range);
        if (2 != count($rangeArray)) {
            return $result;
        }
        foreach ($rangeArray as $time) {
            if (!strtotime($time)) {
                return $result;
            }
            if (0 == count($result['range'])) {
                $result['range']['start'] = $time;
            } else {
                $result['range']['end'] = $time;
            }
        }
        $result['code'] = PublicEnum::ResponseCode['success'];
        $result['message'] = '';
        return $result;
    }

    //日期驗證
    public function checkDateTime($date = ''): array
    {
        $result = [
            'code' => 0,
            'message' => __('web_message.general.dateTime') . ' ' . __('web_message.general.error'),
        ];
        if (empty($date)) {
            return $result;
        }
        if (!strtotime($date)) {
            return $result;
        }
        $result['code'] = PublicEnum::ResponseCode['success'];
        $result['message'] = '';
        return $result;
    }

    //日期快選
    public function fastDateRange($option = '')
    {
        switch ($option) {
            case 'lastMonth':
                $range = Carbon::now()->setTimezone(TimeEnum::TimeZone['backend'])->subMonth()->startOfMonth()->format('Y-m-d') . ' ~ ' . Carbon::now()->setTimezone(TimeEnum::TimeZone['backend'])->subMonth()->endOfMonth()->format('Y-m-d');
                break;
            case 'thisMonth':
                $range = Carbon::now()->setTimezone(TimeEnum::TimeZone['backend'])->startOfMonth()->format('Y-m-d') . ' ~ ' . Carbon::now()->setTimezone(TimeEnum::TimeZone['backend'])->endOfMonth()->format('Y-m-d');
                break;
            case 'lastWeek':
                $range = Carbon::now()->setTimezone(TimeEnum::TimeZone['backend'])->subWeek()->startOfWeek()->format('Y-m-d') . ' ~ ' . Carbon::now()->setTimezone(TimeEnum::TimeZone['backend'])->subWeek()->endOfWeek()->format('Y-m-d');
                break;
            case 'thisWeek':
                $range = Carbon::now()->setTimezone(TimeEnum::TimeZone['backend'])->startOfWeek()->format('Y-m-d') . ' ~ ' . Carbon::now()->setTimezone(TimeEnum::TimeZone['backend'])->endOfWeek()->format('Y-m-d');
                break;
            case 'yesterday':
                $range = Carbon::now()->setTimezone(TimeEnum::TimeZone['backend'])->subDay()->startOfDay()->format('Y-m-d') . ' ~ ' . Carbon::now()->setTimezone(TimeEnum::TimeZone['backend'])->subDay()->endOfDay()->format('Y-m-d');
                break;
            case 'today':
            default:
                $range = Carbon::now()->setTimezone(TimeEnum::TimeZone['backend'])->startOfDay()->format('Y-m-d') . ' ~ ' . Carbon::now()->setTimezone(TimeEnum::TimeZone['backend'])->endOfDay()->format('Y-m-d');
                break;
        }
        return $range;
    }

    //日期時間快選
    public function fastTimeRange($option = '')
    {
        switch ($option) {
            case 'lastMonth':
                $range = Carbon::now()->setTimezone(TimeEnum::TimeZone['backend'])->subMonth()->startOfMonth()->format('Y-m-d H:i') . ' ~ ' . Carbon::now()->setTimezone(TimeEnum::TimeZone['backend'])->subMonth()->endOfMonth()->format('Y-m-d H:i');
                break;
            case 'thisMonth':
                $range = Carbon::now()->setTimezone(TimeEnum::TimeZone['backend'])->startOfMonth()->format('Y-m-d H:i') . ' ~ ' . Carbon::now()->setTimezone(TimeEnum::TimeZone['backend'])->endOfMonth()->format('Y-m-d H:i');
                break;
            case 'lastWeek':
                $range = Carbon::now()->setTimezone(TimeEnum::TimeZone['backend'])->subWeek()->startOfWeek()->format('Y-m-d H:i') . ' ~ ' . Carbon::now()->setTimezone(TimeEnum::TimeZone['backend'])->subWeek()->endOfWeek()->format('Y-m-d H:i');
                break;
            case 'thisWeek':
                $range = Carbon::now()->setTimezone(TimeEnum::TimeZone['backend'])->startOfWeek()->format('Y-m-d H:i') . ' ~ ' . Carbon::now()->setTimezone(TimeEnum::TimeZone['backend'])->endOfWeek()->format('Y-m-d H:i');
                break;
            case 'yesterday':
                $range = Carbon::now()->setTimezone(TimeEnum::TimeZone['backend'])->subDay()->startOfDay()->format('Y-m-d H:i') . ' ~ ' . Carbon::now()->setTimezone(TimeEnum::TimeZone['backend'])->subDay()->endOfDay()->format('Y-m-d H:i');
                break;
            case 'today':
            default:
                $range = Carbon::now()->setTimezone(TimeEnum::TimeZone['backend'])->startOfDay()->format('Y-m-d H:i') . ' ~ ' . Carbon::now()->setTimezone(TimeEnum::TimeZone['backend'])->endOfDay()->format('Y-m-d H:i');
                break;
        }
        return $range;
    }
}
