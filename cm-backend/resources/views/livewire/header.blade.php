<header id="navbar">
    <div id="navbar-container" class="boxed">
        <div class="navbar-header">
            <a href="{{url('dashboard')}}" class="navbar-brand">
                <img src="/resources/img/logo-w.png" alt="Nifty Logo" class="brand-icon">
                <div class="brand-title">
                    <span class="brand-text">{{__('web_message.webSiteTitle')}}</span>
                </div>
            </a>
        </div>
        <div class="navbar-content clearfix">
            <ul class="nav navbar-top-links pull-left">
                <li class="tgl-menu-btn">
                    <a class="mainnav-toggle">
                        <i class="bi-list"></i>
                    </a>
                </li>
            </ul>
            <ul class="nav navbar-top-links pull-right">
                <li class="dropdown">
                    <a id="demo-lang-switch" class="lang-selector dropdown-toggle" data-toggle="dropdown">
                        <span class="lang-selected"><i class="bi-globe"></i></span>
                    </a>
                    <ul class="head-list dropdown-menu">
                        <li>
                            <!--English-->
                            <a wire:click="changeLanguage('en')">
                                <img class="lang-flag" src="/resources/img/us.jpg">
                                <span class="lang-name">{{__('web_message.language.english')}}</span>
                            </a>
                        </li>
                        <li>
                            <!--chinese-->
                            <a wire:click="changeLanguage('zh-cn')">
                                <img class="lang-flag" src="/resources/img/cn.jpg">
                                <span class="lang-name">{{__('web_message.language.chinese')}}</span>
                            </a>
                        </li>
                    </ul>
                </li>
                <li id="dropdown-user" class="dropdown">
                    <a data-toggle="dropdown" class="text-right">
                                <span class="pull-right">
                                    <i class="ic-user bi-person-circle"></i>
                                </span>
                        <div class="username hidden-xs">{{session('userData')->username}}</div>
                    </a>
                    <div class="dropdown-menu dropdown-menu-md dropdown-menu-right panel-default">
                        <div class="pad-all text-right">
                            <a class="btn btn-primary" wire:click="doLogout">
                                <i class="demo-pli-unlock"></i> {{__('web_message.header.logout')}}
                            </a>
                        </div>
                    </div>
                </li>
            </ul>
        </div>
    </div>
    <style>
        .lang-flag {
            width: 2em;
        }
    </style>
</header>
