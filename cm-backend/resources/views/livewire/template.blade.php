<div id="container" class="effect aside-float aside-bright mainnav-lg">
    <livewire:header></livewire:header>
    <div class="boxed">
        <livewire:menu></livewire:menu>
        <div id="content-container">
            @yield('content')
        </div>
    </div>
    <footer id="footer">
        <p class="pad-lft">Copyright &#0169; {{session()->get('nowYear')}} MyBit. All rights reserved.</p>
    </footer>
    @if (session()->has('pagePopMessage'))
        <div class="modal-backdrop fade in" id="pageError" style="display: block;"></div>
        <div class="modal fade in" id="pageErrorContent" style="display: block;">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" onclick="
                            function restorePop(){
                                $('#{{session('popId')}}').addClass('in').css('display','block');
                            }
                            function closeErrorPop() {
                                document.getElementById('pageError').style.display = 'none';
                                document.getElementById('pageErrorContent').style.display = 'none';
                                restorePop();
                            }
                            closeErrorPop()"
                        >
                            <i class="bi-x-circle"></i>
                        </button>
                        <h4 class="modal-title">{{__('web_message.general.remind')}}</h4>
                    </div>
                    <div class="modal-body">
                        <p>{{ session('pagePopMessage') }}</p>
                    </div>
                </div>
            </div>
        </div>
    @endif
</div>
