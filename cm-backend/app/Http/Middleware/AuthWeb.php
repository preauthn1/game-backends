<?php

namespace App\Http\Middleware;

use App\Enums\PublicEnum;
use Closure;
use Illuminate\Http\Request;
use Illuminate\Support\Carbon;
use Illuminate\Support\Facades\App;
use Symfony\Component\HttpFoundation\Response;
use App\Enums\MenuEnum;

class AuthWeb
{
    public function handle(Request $request, Closure $next): Response
    {
        //當前頁面
        $nowUri = $request->getRequestUri() ?? '';
        //權限判斷
        if (!empty(session()->get('userData')->totalUri ?? [])) {
            if (!in_array($nowUri, session()->get('userData')->totalUri)) {
                return redirect('/404');
            }
        }
        session()->put('nowUri', $nowUri);
        //當前頁面標題
        $nowUriTitle = '';
        $menuList = MenuEnum::getMenuList('en') ?? [];
        foreach ($menuList as $title => $menus) {
            if (array_key_exists(substr($nowUri, 1), $menus)) {
                $nowUriTitle = $title;
            }
        }
        session()->put('nowUriTitle', $nowUriTitle);
        //語言
        $language = session()->get('language') ?? config('app.locale');
        App::setLocale($language);
        //今年
        if (empty(session()->get('nowYear') ?? '')) {
            session()->put('nowYear', Carbon::now()->year);
        }
        //預設小數點位數
        session()->put('defaultScale', PublicEnum::DefaultScale);
        //未登入
        $userData = session()->get('userData') ?? [];
        if (empty($userData)) {
            return redirect('/login');
        }
        //重置頁數
        session()->put('nowPage', 1);
        //清除頁面緩存數據
        session()->forget('pageCacheData');
        return $next($request);
    }
}
