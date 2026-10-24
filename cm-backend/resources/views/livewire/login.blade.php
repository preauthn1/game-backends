<div id="container" class="cls-container">
    <div id="bg-overlay" class="bg-img" style="background-image: url('/resources/img/bg-login2.jpg')"></div>
    <div class="cls-content">
        <div>
            @if (session()->has('loginErrorMessage'))
                <div class="alert alert-danger">
                    {{ session('loginErrorMessage') }}
                </div>
            @endif
        </div>
        <div class="cls-content-sm panel">
            <div class="panel-body">
                <div class="logoBlock">
                    <img src="/resources/img/logo-b.png">
                </div>
                <div class="mar-ver" style="margin-top: 0;">
                    <h3 class="h4 mar-no">{{__('web_message.webSiteTitle')}}</h3>
                    <p class="text-muted">{{__('web_message.login.subTitle')}}</p>
                </div>
                <form wire:submit="doLogin">
                    <div class="form-group">
                        <input type="text" class="form-control" placeholder="{{__('web_message.general.username')}}"
                               wire:model="username" autofocus>
                    </div>
                    <div class="form-group">
                        <input type="password" class="form-control" placeholder="{{__('web_message.general.password')}}"
                               wire:model="password">
                    </div>
                    {{--                    <div class="checkbox pad-btm text-left">--}}
                    {{--                        <input id="demo-form-checkbox" class="magic-checkbox" type="checkbox">--}}
                    {{--                        <label for="demo-form-checkbox">{{__('web_message.login.rememberMe')}}</label>--}}
                    {{--                    </div>--}}
                    <button class="btn btn-primary btn-lg btn-block"
                            type="submit">{{__('web_message.login.button')}}</button>
                </form>
            </div>
            <div class="pad-btm">
                <img class="country-flag" wire:click="changeLanguage('en')" src="/resources/img/us.jpg"
                     alt="{{__('web_message.language.english')}}">
                <img class="country-flag" wire:click="changeLanguage('zh-cn')" src="/resources/img/cn.jpg"
                     alt="{{__('web_message.language.chinese')}}">
            </div>
        </div>
    </div>
    <style>
        .logoBlock img {
            width: 60px;
        }

        .country-flag {
            width: 4em;
            display: inline-block;
            margin-right: 1em;
            cursor: pointer;
        }
    </style>
</div>
