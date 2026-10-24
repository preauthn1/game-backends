<ul class="pagination">
    @if((session()->get('pageCacheData')['totalPage']??1)<=7)
        @for ($i = 1; $i <= (session()->get('pageCacheData')['totalPage']??1); $i++)
            <li class="footable-page {{session()->get('nowPage')==$i?'active disabled':''}}">
                <a wire:click="changePage({{session()->get('nowPage')}},{{$i}})">{{$i}}</a>
            </li>
        @endfor
    @else
        <li class="{{session()->get('nowPage')<=1?'disabled':''}}">
            <a wire:click="changePage({{session()->get('nowPage')}},1)">
                <i class="bi-chevron-double-left"></i>
            </a>
        </li>
        <li class="{{session()->get('nowPage')<=1?'disabled':''}}">
            <a wire:click="changePage({{session()->get('nowPage')}},{{session()->get('nowPage')-1}})">
                <i class="bi-chevron-left"></i>
            </a>
        </li>
        <li class="{{session()->get('nowPage')<=1?'active disabled':''}}">
            <a wire:click="changePage({{session()->get('nowPage')}},1)">1</a>
        </li>
        <li class="{{session()->get('nowPage')==2?'active disabled':''}}">
            <a wire:click="changePage({{session()->get('nowPage')}},2)">2</a>
        </li>
        @if(4<session()->get('nowPage'))<li><span>...</span></li>@endif
        @if(3>=session()->get('nowPage'))
            <li class="{{session()->get('nowPage')==3?'active disabled':''}}">
                <a wire:click="changePage({{session()->get('nowPage')}},3)">3</a>
            </li>
            <li class="{{session()->get('nowPage')==4?'active disabled':''}}">
                <a wire:click="changePage({{session()->get('nowPage')}},4)">4</a>
            </li>
            <li class="{{session()->get('nowPage')==5?'active disabled':''}}">
                <a wire:click="changePage({{session()->get('nowPage')}},5)">5</a>
            </li>
        @elseif((session()->get('pageCacheData')['totalPage']-2) <= session()->get('nowPage'))
            <li class="{{session()->get('nowPage')==(session()->get('pageCacheData')['totalPage']-4)?'active disabled':''}}">
                <a wire:click="changePage({{session()->get('nowPage')}},{{session()->get('pageCacheData')['totalPage']-4}})">
                    {{session()->get('pageCacheData')['totalPage']-4}}
                </a>
            </li>
            <li class="{{session()->get('nowPage')==(session()->get('pageCacheData')['totalPage']-3)?'active disabled':''}}">
                <a wire:click="changePage({{session()->get('nowPage')}},{{session()->get('pageCacheData')['totalPage']-3}})">
                    {{session()->get('pageCacheData')['totalPage']-3}}
                </a>
            </li>
            <li class="{{session()->get('nowPage')==(session()->get('pageCacheData')['totalPage']-2)?'active disabled':''}}">
                <a wire:click="changePage({{session()->get('nowPage')}},{{session()->get('pageCacheData')['totalPage']-2}})">
                    {{session()->get('pageCacheData')['totalPage']-2}}
                </a>
            </li>
        @else
            <li class="{{session()->get('nowPage')==(session()->get('nowPage')-1)?'active disabled':''}}">
                <a wire:click="changePage({{session()->get('nowPage')}},{{session()->get('nowPage')-1}})">
                    {{session()->get('nowPage')-1}}
                </a>
            </li>
            <li class="active disabled">
                <a>{{session()->get('nowPage')}}</a>
            </li>
            <li class="{{session()->get('nowPage')==(session()->get('nowPage')+1)?'active disabled':''}}">
                <a wire:click="changePage({{session()->get('nowPage')}},{{session()->get('nowPage')+1}})">
                    {{session()->get('nowPage')+1}}
                </a>
            </li>
        @endif
        @if((session()->get('pageCacheData')['totalPage']-3)>session()->get('nowPage'))<li><span>...</span></li>@endif
        <li class="{{session()->get('nowPage')==(session()->get('pageCacheData')['totalPage']-1)?'active disabled':''}}">
            <a wire:click="changePage({{session()->get('nowPage')}},{{session()->get('pageCacheData')['totalPage']-1}})">
                {{session()->get('pageCacheData')['totalPage']-1}}
            </a>
        </li>
        <li class="{{session()->get('nowPage')==session()->get('pageCacheData')['totalPage']?'active disabled':''}}">
            <a wire:click="changePage({{session()->get('nowPage')}},{{session()->get('pageCacheData')['totalPage']}})">
                {{session()->get('pageCacheData')['totalPage']}}
            </a>
        </li>
        <li class="{{session()->get('nowPage')>=session()->get('pageCacheData')['totalPage']?'disabled':''}}">
            <a wire:click="changePage({{session()->get('nowPage')}},{{session()->get('nowPage')+1}})">
                <i class="bi-chevron-right"></i>
            </a>
        </li>
        <li class="{{session()->get('nowPage')>=session()->get('pageCacheData')['totalPage']?'disabled':''}}">
            <a wire:click="changePage({{session()->get('nowPage')}},{{session()->get('pageCacheData')['totalPage']}})">
                <i class="bi-chevron-double-right"></i>
            </a>
        </li>
    @endif
</ul>
