@extends('livewire.template')
@section('content')
    <div id="page-title">
        <h1 class="page-header text-overflow">{{__('web_message.menu.member.memberDetail')}}</h1>
    </div>
    <div id="page-content">
        <!-- 身分證資料, 充值總額, 提現總額 -->
        <div class="panel">
            <div class="panel-heading">
                <h3 class="panel-title">
                    {{(session()->get('pageCacheData')['memberData']->username??'').' ( '.(session()->get('pageCacheData')['memberData']->uid??'').' ) '}}
                    <button class="btn btn-mint"
                            wire:click="removeLogin({{session()->get('pageCacheData')['memberData']->id??''}})">
                        {{__('web_message.memberDetail.removeLogin')}}
                    </button>
                    @if(9==(session()->get('pageCacheData')['memberData']->status??0))
                        <button class="btn btn-danger"
                                wire:click="unblocked({{session()->get('pageCacheData')['memberData']->id??''}})">
                            {{__('web_message.memberDetail.unblockedMember')}}
                        </button>
                    @elseif(1==(session()->get('pageCacheData')['memberData']->status??0))
                        <button class="btn btn-danger"
                                wire:click="blocked({{session()->get('pageCacheData')['memberData']->id??''}})">
                            {{__('web_message.memberDetail.blockedMember')}}
                        </button>
                    @elseif(2==(session()->get('pageCacheData')['memberData']->status??0))
                        <button class="btn btn-warning"
                                wire:click="passwordUnlock({{session()->get('pageCacheData')['memberData']->id??''}})">
                            {{__('web_message.memberDetail.passwordUnlock')}}
                        </button>
                    @else
                    @endif
                    @if(1==(session()->get('pageCacheData')['memberData']->is_2fa_enable??0))
                    <button class="btn btn-purple"
                            wire:click="resetGoogle2fa({{session()->get('pageCacheData')['memberData']->id??''}})">
                        {{__('web_message.memberDetail.resetGoogle2fa')}}
                    </button>
                    @endif
                </h3>
                <style>
                    .panel-title .div-btn {
                        display: inline-block;
                    }
                </style>
            </div>
            <div class="panel-body">
                <div class="row">
                    <div class="col-lg-11">
                        <div class="row">
                            <div class="w-310">
                                <div class="panel panel-bordered-primary">
                                    <div class="pad-all text-center">
                                        <p>{{__('web_message.memberDetail.id_card_front')}}</p>
                                        <span class="text-3x text-thin">
                                            @if(''==(session()->get('pageCacheData')['memberData']->id_card_front??''))
                                                <div class="mail-file-img">
                                                        <img class="image-responsive" src="" style="height: 100px;">
                                                    </div>
                                            @else
                                                <div class="mail-file-img"
                                                     style="cursor: pointer;"
                                                     onclick="window.open('{{session()->get('pageCacheData')['memberData']->id_card_front}}','_blank');">
                                                        <img class="image-responsive"
                                                             src="{{session()->get('pageCacheData')['memberData']->id_card_front}}"
                                                             style="height: 100px;">
                                                    </div>
                                            @endif
                                        </span>
                                    </div>
                                </div>
                            </div>
                            <div class="w-310">
                                <div class="panel panel-bordered-info">
                                    <div class="pad-all text-center">
                                        <p>{{__('web_message.memberDetail.id_card_back')}}</p>
                                        <span class="text-3x text-thin">
                                            @if(''==(session()->get('pageCacheData')['memberData']->id_card_back??''))
                                                <div class="mail-file-img">
                                                        <img class="image-responsive" src="" style="height: 100px;">
                                                    </div>
                                            @else
                                                <div class="mail-file-img"
                                                     style="cursor: pointer;"
                                                     onclick="window.open('{{session()->get('pageCacheData')['memberData']->id_card_back}}','_blank');">
                                                        <img class="image-responsive"
                                                             src="{{session()->get('pageCacheData')['memberData']->id_card_back}}"
                                                             style="height: 100px;">
                                                    </div>
                                            @endif
                                        </span>
                                    </div>
                                </div>
                            </div>
                            <div class="w-310">
                                <div class="panel panel-bordered-success">
                                    <div class="pad-all text-center">
                                        <p>{{__('web_message.memberDetail.totalDep')}}</p>
                                        <span class="text-3x text-thin" tooltip="{{floatval(session()->get('pageCacheData')['memberData']->total_deposit)}}">
                                            {{number_format(session()->get('pageCacheData')['memberData']->total_deposit??0,session()->get('defaultScale'))}}
                                        </span>
                                    </div>
                                </div>
                            </div>
                            <div class="w-310">
                                <div class="panel panel-bordered-mint">
                                    <div class="pad-all text-center">
                                        <p>{{__('web_message.memberDetail.totalWd')}}</p>
                                        <span class="text-3x text-thin" tooltip="{{floatval(session()->get('pageCacheData')['memberData']->total_withdrawal)}}">
                                            {{number_format(session()->get('pageCacheData')['memberData']->total_withdrawal??0,session()->get('defaultScale'))}}
                                        </span>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
        <!-- 充值明細 -->
        <div class="panel">
            <div class="panel-heading">
                <h3 class="panel-title">
                    <span style="cursor: pointer;"
                          wire:click="doDepositList">{{__('web_message.memberDetail.depositList')}}</span>
                </h3>
            </div>
        </div>
        <!-- 提現明細 -->
        <div class="panel">
            <div class="panel-heading">
                <h3 class="panel-title">
                    <span style="cursor: pointer;"
                          wire:click="doWithdrawalList">{{__('web_message.memberDetail.withdrawalList')}}</span>
                </h3>
            </div>
        </div>
        <!--餘額帳變紀錄-->
        <div class="panel">
            <div class="panel-heading">
                <h3 class="panel-title">{{__('web_message.memberDetail.transactionLog')}}</h3>
            </div>
            <div class="panel-body">
                <form class="form-inline" wire:submit="doSearch" style="position: relative;">
                    <div class="form-group">
                        <label>{{__('web_message.general.type')}}：</label>
                        <select class="form-select form-control" wire:model="type">
                            <option value="-1">{{__('web_message.general.select')}}</option>
                            @forelse((session()->get('pageCacheData')['typeList']??[]) as $data)
                                <option value="{{$data->id}}">{{$data->name}}</option>
                            @empty
                            @endforelse
                        </select>
                    </div>
                    <button class="btn btn-primary" type="submit">{{__('web_message.general.search')}}</button>
                </form>
                <div class="table-responsive">
                    <table class="table table-striped txt-center">
                        <thead>
                        <tr>
                            <th>{{__('web_message.general.orderNo')}}</th>
                            <th>{{__('web_message.general.type')}}</th>
                            <th>{{__('web_message.general.amount')}}</th>
                            <th>{{__('web_message.general.before')}}{{__('web_message.general.amount')}}</th>
                            <th>{{__('web_message.general.after')}}{{__('web_message.general.amount')}}</th>
                            <th>{{__('web_message.general.createdAt')}}</th>
                        </tr>
                        </thead>
                        <tbody>
                        @forelse((session()->get('pageCacheData')['pageData']??[]) as $data)
                            <tr>
                                <td>{{$data->id}}</td>
                                <td>{{$data->transaction_type}}</td>
                                <td tooltip="{{floatval($data->point)}}">{{number_format($data->point??0,session()->get('defaultScale'))}}</td>
                                <td tooltip="{{floatval($data->before_balance)}}">{{number_format($data->before_balance??0,session()->get('defaultScale'))}}</td>
                                <td tooltip="{{floatval($data->after_balance)}}">{{number_format($data->after_balance??0,session()->get('defaultScale'))}}</td>
                                <td>{{$data->created_at??''}}</td>
                            </tr>
                        @empty
                            <tr>
                                <td colspan="6">{{__('web_message.general.noData')}}</td>
                            </tr>
                        @endforelse
                        </tbody>
                        <tfoot>
                        <tr>
                            <td colspan="6" class="footable-visible">
                                <div class="text-right" style="position: relative;">
                                    <div class="total-entries">
                                        {{(session()->get('pageCacheData')['totalRow']??0)}} {{__('web_message.general.entries')}}
                                    </div>
                                    <x-pagination></x-pagination>
                                </div>
                            </td>
                        </tr>
                        </tfoot>
                    </table>
                </div>
            </div>
        </div>
        <style>
            div.w-310 {
                width: 310px;
            }

            div.w-310 .panel {
                min-height: 165px;
            }
        </style>
    </div>
@endsection
