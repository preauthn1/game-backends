@extends('livewire.template')
@section('content')
    <div id="page-title">
        <h1 class="page-header text-overflow">{{__('web_message.menu.deposit.depositList')}}</h1>
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
                <div class="table-responsive">
                    <table class="table table-striped txt-center">
                        <thead>
                        <tr>
                            <th></th>
                            <th>{{__('web_message.general.status')}}</th>
                            <th>{{__('web_message.general.username')}}</th>
                            <th>{{__('web_message.general.channel')}}</th>
                            <th>{{__('web_message.general.orderNo')}}</th>
                            <th>{{__('web_message.general.amount')}}</th>
                            <th>{{__('web_message.general.fxRate')}}</th>
                            <th>{{__('web_message.general.finalAmount')}}</th>
                            <th>{{__('web_message.general.createdAt')}}</th>
                            <th>{{__('web_message.general.payTime')}}</th>
                        </tr>
                        </thead>
                        <tbody>
                        @forelse((session()->get('pageCacheData')['pageData']??[]) as $data)
                            <tr>
                                <td>
                                    @if(3==$data->status)
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
                                <td>{{$data->gatewayTxt}}</td>
                                <td>{{$data->order_no}}</td>
                                <td tooltip="{{floatval($data->amount)}}">{{number_format($data->amount,session()->get('defaultScale'))}}</td>
                                <td>{{rtrim(rtrim($data->exchange_rate??0, '0'), '.')}}</td>
                                <td tooltip="{{floatval($data->point)}}">{{number_format($data->point,session()->get('defaultScale'))}}</td>
                                <td>{{$data->created_at}}</td>
                                <td>{{$data->pay_time??''}}</td>
                            </tr>
                        @empty
                            <tr>
                                <td colspan="10">{{__('web_message.general.noData')}}</td>
                            </tr>
                        @endforelse
                        </tbody>
                        <tfoot>
                        <tr>
                            <td colspan="10" class="footable-visible">
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
        <!-- depositPop -->
        <div class="modal fade in" id="depositPop" role="dialog"
             style="display: {{'depositPop' == session('showPopId') ? 'block':'none'}};">
            <div class="modal-backdrop fade in" style="display: block;"></div>
            <div class="modal-dialog" style="z-index: 1050;margin: 15px 0;">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" id="popClose">&times;</button>
                        <h4 class="modal-title">{{'modify'==(session()->get('pageCacheData')['popAction']??'')?__('web_message.menu.deposit.modifyDeposit'):__('web_message.menu.deposit.depositDetail')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal">
                            <div class="panel-body" style="padding-bottom: 10px;">
                                <input type="hidden" wire:model="modifyDepositId">
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">{{__('web_message.general.username')}}</label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popUsername" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">{{__('web_message.general.channel')}}</label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popChannel" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">{{__('web_message.general.orderNo')}}</label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popOrderNo" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">{{__('web_message.general.amount')}}</label>
                                    <div class="col-sm-6" tooltip="{{floatval(str_replace(',', '', $popAmount))}}">
                                        <input type="text" class="form-control" wire:model="popAmount" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">{{__('web_message.general.fxRate')}}</label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popRate" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label
                                        class="col-sm-4 control-label">{{__('web_message.general.finalAmount')}}</label>
                                    <div class="col-sm-6" tooltip="{{floatval(str_replace(',', '', $popPoint))}}">
                                        <input type="text" class="form-control" wire:model="popPoint" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label
                                        class="col-sm-4 control-label">{{__('web_message.general.image')}}</label>
                                    <div class="col-sm-8">
                                        <ul class="mail-attach-list list-ov">
                                            <li>
                                                @if(''==$popImage)
                                                    <div class="mail-file-img"
                                                         style="border: 1px solid #ddd;height: 100px;">
                                                        <img class="image-responsive" src="" style="width: 170px;">
                                                    </div>
                                                @else
                                                    <div class="mail-file-img"
                                                         style="border: 1px solid #ddd;cursor: pointer;height: 100px;"
                                                         onclick="window.open('{{$popImage}}','_blank');">
                                                        <img class="image-responsive" src="{{$popImage}}"
                                                             style="width: 170px;">
                                                    </div>
                                                @endif
                                            </li>
                                        </ul>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-4 control-label">{{__('web_message.general.status')}}：</label>
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
                                    <label
                                        class="col-sm-4 control-label">{{__('web_message.general.lastOperator')}}</label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popOperator" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label
                                        class="col-sm-4 control-label">{{__('web_message.general.lastUpdateTime')}}</label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popUpdateAt" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label
                                        class="col-sm-4 control-label">{{__('web_message.general.payTime')}}</label>
                                    <div class="col-sm-6">
                                        <input type="text" class="form-control" wire:model="popPayTime" disabled>
                                    </div>
                                </div>
                                <div class="form-group" style="margin-bottom: 0;">
                                    <label class="col-sm-4 control-label">{{__('web_message.general.remark')}}：</label>
                                    <div class="col-sm-8">
                                    <textarea rows="5" class="form-control"
                                              wire:model="modifyDepositRemark" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}></textarea>
                                    </div>
                                </div>
                            </div>
                        </form>
                    </div>
                    @if('modify'==(session()->get('pageCacheData')['popAction']??''))
                        <div class="panel-footer">
                            <div class="row">
                                <div style="text-align: center;">
                                    <div class="btn btn-mint" wire:click="doModify('approved')">
                                        <i class="bi-check2-square"></i> {{__('web_message.general.approved')}}
                                    </div>
                                    <div class="btn btn-danger offset-1" wire:click="doModify('refused')">
                                        <i class="bi-x-square"></i> {{__('web_message.general.refused')}}
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
                    $('#depositPop').removeClass('in').css('display', 'none');
                    $('.modal-backdrop').css('display', 'none');
                });
            });
        </script>
    </div>
@endsection
