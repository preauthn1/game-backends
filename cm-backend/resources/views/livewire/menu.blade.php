<nav id="mainnav-container">
    <div id="mainnav">
        <div id="mainnav-menu-wrap">
            <div class="nano has-scrollbar">
                <div class="nano-content" tabindex="0" style="right: -17px;">
                    <ul id="mainnav-menu" class="list-group">
                        @forelse((session()->get('userData')->menu??[]) as $list)
                            @if($list->count>0)
                                <li>
                                    <a href="#">
                                        <i class="{{$list->symbol}}"></i>
                                        <span class="menu-title">{{__('web_message.menu.'.$list->name.'.title')}}</span>
                                        <i class="arrow"></i>
                                    </a>
                                    <!--Submenu-->
                                    @forelse(($list->sub??[]) as $sub)
                                        @if($sub->is_show)
                                            <ul class="collapse {{session('nowUriTitle')==$list->name?'in':''}}">
                                                <li class="{{session('nowUri')==$sub->uri?'active-link':''}}">
                                                    <a href="{{url($sub->uri)}}">
                                                <span
                                                    class="menu-title">{{__('web_message.menu.'.$list->name.'.'.$sub->name)}}</span>
                                                    </a>
                                                </li>
                                            </ul>
                                        @endif
                                    @empty
                                    @endforelse
                                </li>
                                <li class="list-divider"></li>
                            @endif
                        @empty
                        @endforelse

                    </ul>
                </div>
            </div>
        </div>
    </div>
</nav>
