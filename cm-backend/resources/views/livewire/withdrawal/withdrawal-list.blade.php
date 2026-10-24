@extends('livewire.template')
@section('content')
    <div id="page-title">
        <h1 class="page-header text-overflow">{{__('web_message.menu.withdrawal.withdrawalList')}}</h1>
    </div>
    <div id="page-content">
        <!--搜索區塊-->
        <div class="panel">
            <div class="panel-heading">
                <h3 class="panel-title">{{__('web_message.general.search')}}</h3>
            </div>
            <div class="panel-body">
                <form class="form-inline" wire:submit="doSearch" style="position: relative;">
                    <div class="form-group">
                        <label>{{__('web_message.general.username')}}：</label>
                        <input type="text" class="form-control" wire:model="username">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.channel')}}：</label>
                        <select class="form-select form-control" wire:model="channel">
                            <option value="-1">{{__('web_message.general.select')}}</option>
                            @forelse((session()->get('pageCacheData')['channelList']??[]) as $data)
                                <option value="{{$data->id}}">{{$data->name}}</option>
                            @empty
                            @endforelse
                        </select>
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.orderNo')}}：</label>
                        <input type="text" class="form-control" wire:model="orderNo">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.status')}}：</label>
                        <select class="form-select form-control" wire:model="status">
                            <option value="-1">{{__('web_message.general.select')}}</option>
                            @forelse((session()->get('pageCacheData')['statusList']??[]) as $data)
                                <option value="{{$data->id}}">{{$data->name}}</option>
                            @empty
                            @endforelse
                        </select>
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.amountRange')}}：</label>
                        <input type="text" class="form-control" wire:model="amountMin" style="width: 100px;"
                               oninput="value=value.replace(/\D/g,'')">
                        ~
                        <input type="text" class="form-control" wire:model="amountMax" style="width: 100px;"
                               oninput="value=value.replace(/\D/g,'')">
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.createdAt')}}：</label>
                        <div class="time-range">
                            <input type="text" id="orderTimeRange" class="form-control" wire:model="orderTimeRange" autocomplete="off"
                                   placeholder="{{__('web_message.general.select')}} {{__('web_message.general.timeRange')}}"/>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setOrderTimeRange('today')">
                                {{__('web_message.fastTime.today')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setOrderTimeRange('yesterday')">
                                {{__('web_message.fastTime.yesterday')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setOrderTimeRange('thisWeek')">
                                {{__('web_message.fastTime.thisWeek')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setOrderTimeRange('lastWeek')">
                                {{__('web_message.fastTime.lastWeek')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setOrderTimeRange('thisMonth')">
                                {{__('web_message.fastTime.thisMonth')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setOrderTimeRange('lastMonth')">
                                {{__('web_message.fastTime.lastMonth')}}
                            </u>
                        </div>
                    </div>
                    <div class="form-group">
                        <label>{{__('web_message.general.payTime')}}：</label>
                        <div class="time-range">
                            <input type="text" id="payTimeRange" class="form-control" wire:model="payTimeRange" autocomplete="off"
                                   placeholder="{{__('web_message.general.select')}} {{__('web_message.general.timeRange')}}"/>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setPayTimeRange('today')">
                                {{__('web_message.fastTime.today')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setPayTimeRange('yesterday')">
                                {{__('web_message.fastTime.yesterday')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setPayTimeRange('thisWeek')">
                                {{__('web_message.fastTime.thisWeek')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setPayTimeRange('lastWeek')">
                                {{__('web_message.fastTime.lastWeek')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setPayTimeRange('thisMonth')">
                                {{__('web_message.fastTime.thisMonth')}}
                            </u>
                            <u class="fast-time btn btn-default btn-rounded" wire:click="setPayTimeRange('lastMonth')">
                                {{__('web_message.fastTime.lastMonth')}}
                            </u>
                        </div>
                    </div>
                    <button class="btn btn-primary btn-search"
                            type="submit">{{__('web_message.general.search')}}</button>
                </form>
            </div>
        </div>
        <!--資料區塊-->
        <div class="panel">
            <div class="panel-body">
                <div class="pad-btm form-inline">
                    <div class="row">
                        <div class="col-sm-6 table-toolbar-left">
                            <button class="btn btn-purple" wire:click="showAddWithdrawal()">
                                <i class="bi-plus-circle"></i> {{__('web_message.general.add')}}
                            </button>
                        </div>
                    </div>
                </div>
                <div class="table-responsive">
                    <table class="table table-striped txt-center">
                        <thead>
                        <tr>
                            <th></th>
                            <th>{{__('web_message.general.status')}}</th>
                            <th>{{__('web_message.general.username')}}</th>
                            <th>{{__('web_message.general.memberType.title')}}</th>
                            <th>{{__('web_message.general.channel')}}</th>
                            <th>{{__('web_message.general.orderNo')}}</th>
                            <th>{{__('web_message.general.amount')}}</th>
                            <th>{{__('web_message.general.fxRate')}}</th>
                            <th>{{__('web_message.general.withdrawalFee')}}</th>
                            <th>{{__('web_message.general.finalAmount')}}</th>
                            <th>{{__('web_message.general.createdAt')}}</th>
                            <th>{{__('web_message.general.payTime')}}</th>
                        </tr>
                        </thead>
                        <tbody>
                        @forelse((session()->get('pageCacheData')['pageData']??[]) as $data)
                            <tr>
                                <td>
                                    @if(in_array($data->status,[0,3]))
                                        <button class="btn btn-warning"
                                                wire:click="showDataPop({{$data->id}},'modify')">
                                            <i class="bi-list-task"></i>
                                        </button>
                                    @else
                                        <button class="btn btn-warning"
                                                wire:click="showDataPop({{$data->id}},'detail')">
                                            <i class="bi-list-task"></i>
                                        </button>
                                    @endif
                                </td>
                                <td>{{$data->statusTxt}}</td>
                                @if(!empty($data->memo))
                                    <td class='has-memo' tooltip="{{__('web_message.general.remark').'： '.$data->memo}}">
                                        {{$data->username}}
                                    </td>
                                @else
                                    <td>{{$data->username}}</td>
                                @endif
                                <td>{{$data->memberTypeTxt}}</td>
                                <td>{{$data->gatewayTxt}}</td>
                                <td>{{$data->order_no}}</td>
                                <td tooltip="{{floatval($data->point)}}">{{number_format($data->point,session()->get('defaultScale'))}}</td>
                                <td>{{rtrim(rtrim($data->exchange_rate??0, '0'), '.')}}</td>
                                <td tooltip="{{floatval($data->fee)}}">{{number_format($data->fee,session()->get('defaultScale'))}}</td>
                                <td tooltip="{{floatval($data->amount)}}">{{number_format($data->amount, session()->get('defaultScale'))}}</td>
                                <td>{{$data->created_at}}</td>
                                <td>{{$data->pay_time??''}}</td>
                            </tr>
                        @empty
                            <tr>
                                <td colspan="12">{{__('web_message.general.noData')}}</td>
                            </tr>
                        @endforelse
                        </tbody>
                        <tfoot>
                        <tr>
                            <td colspan="12" class="footable-visible">
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
                    <style>
                        td.has-memo {
                            color: red !important;
                            position: relative;
                            cursor: pointer;
                        }
                    </style>
                </div>
            </div>
        </div>
        <!-- addWithdrawal -->
        <div class="modal fade in" id="addWithdrawal" role="dialog"
            style="display: {{'addWithdrawal' == session('showPopId') ? 'block':'none'}};">
            <div class="modal-backdrop fade in" style="display: block;"></div>
            <div class="modal-dialog" style="z-index: 1050;">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" id="addClose">&times;</button>
                        <h4 class="modal-title">{{__('web_message.menu.withdrawal.addWithdrawal')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal" wire:submit="addWithdrawal">
                            <div class="panel-body">
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        <span class="required">*</span>{{__('web_message.general.username')}}
                                    </label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control" wire:model="addUsername">
                                    </div>
                                    <div class="col-sm-3">
                                        <div class="btn btn-default" wire:click="getUserWithdrawalData()">{{__('web_message.general.memberData')}}</div>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">{{__('web_message.general.balance')}}</label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control" wire:model="addBalance" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        <span class="required">*</span>{{__('web_message.general.channel')}}
                                    </label>
                                    <div class="col-sm-7">
                                        <select class="form-select form-control" wire:model="addChannel" wire:change="switchWithdrawlBank">
                                            <option value="-1">{{__('web_message.general.select')}}</option>
                                            @forelse((session()->get('pageCacheData')['channelList']??[]) as $data)
                                                <option value="{{$data->id}}">{{$data->name}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">{{__('web_message.general.withdrawalBankName')}}</label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="addBankName" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">{{__('web_message.general.withdrawalAccountNo')}}</label>
                                    <div class="col-sm-7">
                                        <input type="text" class="form-control" wire:model="addAccountNo" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">{{__('web_message.general.withdrawalFeeRate').'(%)'}}</label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control" wire:model="addTaxRate" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">{{__('web_message.general.fxRate')}}</label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control" wire:model="addRate" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">{{__('web_message.general.amountRange')}}</label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" wire:model="addAmountTip" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        <span class="required">*</span>{{__('web_message.general.amount')}}
                                    </label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control" wire:model="addAmount" oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')">
                                    </div>
                                    <div class="col-sm-3">
                                        <div class="btn btn-default" wire:click="calAmount()">{{__('web_message.general.calculate')}}</div>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">{{__('web_message.general.withdrawalFee')}}</label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control" wire:model="addTax" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">{{__('web_message.general.finalAmount')}}</label>
                                    <div class="col-sm-4">
                                        <input type="text" class="form-control" wire:model="addRealAmount" disabled>
                                    </div>
                                </div>
                            </div>
                            <div class="panel-footer">
                                <div class="row">
                                    <div class="col-sm-offset-10">
                                        <button class="btn btn-mint"
                                                type="submit">{{__('web_message.general.add')}}</button>
                                    </div>
                                </div>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
        <!-- dataPop -->
        <div class="modal fade in" id="dataPop" role="dialog"
             style="display: {{'dataPop' == session('showPopId') ? 'block':'none'}};">
            <div class="modal-backdrop fade in" style="display: block;"></div>
            <div class="modal-dialog" style="z-index: 1050;">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" id="popClose">&times;</button>
                        <h4 class="modal-title">{{'modify'==(session()->get('pageCacheData')['popAction']??'')?__('web_message.menu.withdrawal.modifyWithdrawal'):__('web_message.menu.withdrawal.withdrawalDetail')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal" wire:submit="doModify">
                            <div class="panel-body">
                                <input type="hidden" wire:model="modifyDataId">
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        {{__('web_message.general.username')}}
                                    </label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popUsername" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        {{__('web_message.general.memberType.title')}}
                                    </label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popUserTypeTxt" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        {{__('web_message.general.channel')}}
                                    </label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popChannel" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        {{__('web_message.general.orderNo')}}
                                    </label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popOrderNo" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        {{__('web_message.general.depAccountName')}}
                                    </label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popCardHolder" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        {{__('web_message.general.depAccountNo')}}
                                    </label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popCardNo" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        {{__('web_message.general.amount')}}
                                    </label>
                                    <div class="col-sm-6" tooltip="{{floatval(str_replace(',', '', $popPoint))}}">
                                        <input type="text" class="form-control" wire:model="popPoint" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        {{__('web_message.general.fxRate')}}
                                    </label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popRate" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        {{__('web_message.general.withdrawalFee')}}
                                    </label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popFee" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        {{__('web_message.general.finalAmount')}}
                                    </label>
                                    <div class="col-sm-6" tooltip="{{floatval(str_replace(',', '', $popAmount))}}">
                                        <input type="text" class="form-control" wire:model="popAmount" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        {{__('web_message.general.status')}}
                                    </label>
                                    <div class="col-sm-6">
                                        <select class="form-select form-control"
                                                wire:model="popStatus" disabled>
                                            <option value="-1">{{__('web_message.general.select')}}</option>
                                            @forelse((session()->get('pageCacheData')['statusList']??[]) as $data)
                                                <option value="{{$data->id}}">{{$data->name}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        {{__('web_message.general.lastOperator')}}
                                    </label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popOperator" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        {{__('web_message.general.lastUpdateTime')}}
                                    </label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popUpdateAt" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label
                                        class="col-sm-5 control-label">{{__('web_message.general.payTime')}}</label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popPayTime" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-5 control-label">
                                        {{__('web_message.general.remark')}}
                                    </label>
                                    <div class="col-sm-7">
                                    <textarea rows="5" class="form-control"
                                              wire:model="modifyDataRemark" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}></textarea>
                                    </div>
                                </div>
                            </div>
                        </form>
                    </div>
                    @if('modify'==(session()->get('pageCacheData')['popAction']??''))
                        <div class="panel-footer">
                            <div class="row">
                                <div style="text-align: center;">
                                    @if(0==$popStatus)
                                        <div class="btn btn-mint" wire:click="doModify('approved')">
                                            <i class="bi-check2-square"></i> {{__('web_message.general.approved')}}
                                        </div>
                                    @endif
                                    @if(3==$popStatus)
                                        <div class="btn btn-mint" wire:click="doModify('payed')">
                                            <i class="bi-check2-square"></i> {{__('web_message.general.payed')}}
                                        </div>
                                    @endif
                                    <div class="btn btn-danger" style="margin-left: 3%;"
                                         wire:click="doModify('canceledThenBack')">
                                        <i class="bi-x-square"></i> {{__('web_message.general.canceledThenBack')}}
                                    </div>
                                    <div class="btn btn-pink" style="margin-left: 3%;"
                                         wire:click="doModify('canceledNotBack')">
                                        <i class="bi-x-square-fill"></i> {{__('web_message.general.canceledNotBack')}}
                                    </div>
                                </div>
                            </div>
                        </div>
                    @endif
                </div>
            </div>
        </div>
        <!--頁面特殊JS-->
        <script>
            $(function () {
                $('#orderTimeRange').daterangepicker({
                    timePicker: true,
                    timePicker24Hour: true,
                    autoUpdateInput: false,
                });
                $('#orderTimeRange').on('apply.daterangepicker', function (ev, picker) {
                    let range = picker.startDate.format('YYYY-MM-DD HH:mm') + ' ~ ' + picker.endDate.format('YYYY-MM-DD HH:mm');
                    $(this).val(range);
                    Livewire.dispatch('orderTimeRange', {range: range});
                });
                $('#orderTimeRange').on('cancel.daterangepicker', function (ev, picker) {
                    let range = '';
                    $(this).val(range);
                    Livewire.dispatch('orderTimeRange', {range: range});
                });
                $('#payTimeRange').daterangepicker({
                    timePicker: true,
                    timePicker24Hour: true,
                    autoUpdateInput: false,
                });
                $('#payTimeRange').on('apply.daterangepicker', function (ev, picker) {
                    let range = picker.startDate.format('YYYY-MM-DD HH:mm') + ' ~ ' + picker.endDate.format('YYYY-MM-DD HH:mm');
                    $(this).val(range);
                    Livewire.dispatch('payTimeRange', {range: range});
                });
                $('#payTimeRange').on('cancel.daterangepicker', function (ev, picker) {
                    let range = '';
                    $(this).val(range);
                    Livewire.dispatch('payTimeRange', {range: range});
                });
                $('#popClose').click(function () {
                    $('#dataPop').removeClass('in').css('display', 'none');
                    $('.modal-backdrop').css('display', 'none');
                });
                $('#addClose').click(function () {
                    $('#addWithdrawal').removeClass('in').css('display', 'none');
                    $('.modal-backdrop').css('display', 'none');
                });
            });
        </script>
    </div>
@endsection
