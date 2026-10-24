@extends('livewire.template')
@section('content')
<div id="page-title">
    <h1 class="page-header text-overflow">{{__('web_message.menu.system.depositSetting')}}</h1>
</div>
<div id="page-content">
    <div class="panel">
        <div class="panel-heading">
            <h3 class="panel-title">{{__('web_message.general.setting')}}{{__('web_message.general.faitMoney')}} to
                USDT{{__('web_message.general.fxRate')}}</h3>
        </div>
        <div class="panel-body">
            <form class="form-inline" wire:submit="updateDepositRate" style="position: relative;">
                <div class="form-group group-change">
                    <input type="text" class="form-control fix-input" value="1" disabled>
                    <select class="form-select form-control" wire:model="rateCurrencyId" wire:change="changeRate">
                        <option value="0">
                            {{__('web_message.general.select')}} {{__('web_message.general.currency')}}
                        </option>
                        @forelse ((session()->get('pageCacheData')['currencyList']??[]) as $data)
                        <option value="{{$data['id']}}"
                            name="{{$data['deposit_rate']}}">{{$data['name']}}</option>
                        @empty
                        @endforelse
                    </select>
                </div>
                <div class="form-group"><i class="bi-arrow-left-right" style="margin: 0 10px;"></i></div>
                <div class="form-group group-change">
                    <input type="text" placeholder="{{__('web_message.general.fxRate')}}" class="form-control"
                        wire:model="fxRate" style="width: 140px;"
                        oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d\d\d\d\d\d\d\d\d).*$/,'$1.$2')">
                    <select class="form-select form-control" disabled>
                        <option selected>USDT</option>
                    </select>
                </div>
                <button class="btn btn-primary btn-search mx-3"
                    type="submit">{{__('web_message.general.setting')}}
                </button>
            </form>
        </div>
    </div>
    <!--收款銀行設置 搜索區塊-->
    <div class="panel">
        <div class="panel-heading">
            <h3 class="panel-title">{{__('web_message.depositSetting.depBankSetting')}}</h3>
        </div>
        <div class="panel-body">
            <form class="form-inline" wire:submit="doSearch" style="position: relative;">
                <div class="form-group">
                    <label>{{__('web_message.general.type')}}：</label>
                    <select class="form-select form-control" wire:model="depBankType">
                        <option value="0">
                            {{__('web_message.general.select')}}
                        </option>
                        @forelse ((session()->get('pageCacheData')['typeList']??[]) as $data)
                        <option value="{{$data->id}}">{{$data->name}}</option>
                        @empty
                        @endforelse
                    </select>
                </div>
                <div class="form-group">
                    <label>{{__('web_message.general.currency')}}：</label>
                    <select class="form-select form-control" wire:model="depBankCurrency">
                        <option value="0">
                            {{__('web_message.general.select')}}
                        </option>
                        @forelse ((session()->get('pageCacheData')['currencyList']??[]) as $data)
                        <option value="{{$data['id']}}"
                            name="{{$data['deposit_rate']}}">{{$data['name']}}</option>
                        @empty
                        @endforelse
                    </select>
                </div>
                <div class="form-group">
                    <label>{{__('web_message.general.depBankName')}}：</label>
                    <input type="text" class="form-control" wire:model="depBankName">
                </div>
                <div class="form-group">
                    <label>{{__('web_message.general.status')}}：</label>
                    <select class="form-select form-control" wire:model="depBankStatus">
                        <option value="-1">
                            {{__('web_message.general.select')}}
                        </option>
                        @forelse ((session()->get('pageCacheData')['statusList']??[]) as $data)
                        <option value="{{$data->id}}">{{$data->name}}</option>
                        @empty
                        @endforelse
                    </select>
                </div>
                <button class="btn btn-primary" type="submit">{{__('web_message.general.search')}}</button>
            </form>
        </div>
    </div>
    <!--收款銀行設置 資料區塊-->
    <div class="panel">
        <div class="panel-body">
            <div class="pad-btm form-inline">
                <div class="row">
                    <div class="col-sm-6 table-toolbar-left">
                        <button class="btn btn-purple" data-toggle="modal" data-target="#addDepBank">
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
                            <th>ID</th>
                            <th>{{__('web_message.general.status')}}</th>
                            <th>{{__('web_message.general.type')}}</th>
                            <th>{{__('web_message.general.currency')}}</th>
                            <th>{{__('web_message.general.depBankName')}}</th>
                            <th>{{__('web_message.general.alias')}}</th>
                            <th>{{__('web_message.general.minAmount')}}</th>
                            <th>{{__('web_message.general.maxAmount')}}</th>
                            <th>{{__('web_message.menuMgmt.order')}}</th>
                        </tr>
                    </thead>
                    <tbody>
                        @forelse((session()->get('pageCacheData')['pageData']??[]) as $data)
                        <tr>
                            <td>
                                <button class="btn btn-warning"
                                    wire:click="showDataPop({{$data->id}},'modify')">
                                    <i class="bi-pencil-square"></i>
                                </button>
                                <button class="btn btn-danger"
                                    wire:click="showDataPop({{$data->id}},'detail')">
                                    <i class="bi-list-task"></i>
                                </button>
                            </td>
                            <td>{{$data->id??''}}</td>
                            <td style="font-size: 16px;">
                                <i class="{{(1==$data->status)?'bi-toggle-on':'bi-toggle-off'}}"></i>
                            </td>
                            <td>{{$data->gatewayTxt??''}}</td>
                            <td>{{$data->currency_name??''}}</td>
                            <td>{{$data->bank_name??''}}</td>
                            <td>{{$data->bank_code??''}}</td>
                            <td>{{$data->min_amount??''}}</td>
                            <td>{{$data->max_amount??''}}</td>
                            <td>{{$data->sort??''}}</td>
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
            </div>
        </div>
    </div>
    <!-- 新增收款銀行 -->
    <div class="modal fade" id="addDepBank" role="dialog">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                    <h4 class="modal-title">{{__('web_message.depositSetting.addDepBank')}}</h4>
                </div>
                <div class="modal-body">
                    <form class="form-horizontal" wire:submit="addDepBank">
                        <div class="panel-body">
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    <span class="required">*</span>{{__('web_message.general.type')}}
                                </label>
                                <div class="col-sm-7">
                                    <select class="form-select form-control" wire:model="addDepBankTypeId">
                                        <option value="0">
                                            {{__('web_message.general.select')}}
                                        </option>
                                        @forelse ((session()->get('pageCacheData')['typeList']??[]) as $data)
                                        <option value="{{$data->id}}">{{$data->name}}</option>
                                        @empty
                                        @endforelse
                                    </select>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    <span class="required">*</span>{{__('web_message.general.currency')}}
                                </label>
                                <div class="col-sm-5">
                                    <select class="form-select form-control" wire:model="addDepBankCurrencyId">
                                        <option value="0">
                                            {{__('web_message.general.select')}}
                                        </option>
                                        @forelse ((session()->get('pageCacheData')['depositCurrencyList']??[]) as $data)
                                        <option value="{{$data['id']}}">{{$data['name']}}</option>
                                        @empty
                                        @endforelse
                                    </select>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    <span class="required">*</span>{{__('web_message.general.depBankName')}}
                                </label>
                                <div class="col-sm-5">
                                    <input type="text" class="form-control" wire:model="addDepBankName">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">{{__('web_message.general.alias')}}</label>
                                <div class="col-sm-5">
                                    <input type="text" class="form-control" wire:model="addDepBankCode">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    <span class="required">*</span>{{__('web_message.general.minAmount')}}
                                </label>
                                <div class="col-sm-4">
                                    <input type="text" class="form-control" wire:model="addDepBankMinAmount"
                                        oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    <span class="required">*</span>{{__('web_message.general.maxAmount')}}
                                </label>
                                <div class="col-sm-4">
                                    <input type="text" class="form-control" wire:model="addDepBankMaxAmount"
                                        oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">{{__('web_message.menuMgmt.order')}}</label>
                                <div class="col-sm-3">
                                    <input type="text" class="form-control" wire:model="addDepBankOrder" oninput="value=value.replace(/\D/g,'')">
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
    <!-- depBankPop -->
    <div class="modal fade in" id="depBankPop" role="dialog"
        style="display: {{'depBankPop' == session('showPopId') ? 'block':'none'}};">
        <div class="modal-backdrop fade in" style="display: block;"></div>
        <div class="modal-dialog" style="z-index: 1050;">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" id="depBankPopClose">&times;</button>
                    <h4 class="modal-title">{{'modify'==(session()->get('pageCacheData')['popAction']??'')?__('web_message.depositSetting.modifyDepBank'):__('web_message.depositSetting.depBankDetail')}}</h4>
                </div>
                <div class="modal-body">
                    <form class="form-horizontal" wire:submit="doModify">
                        <div class="panel-body">
                            <input type="hidden" wire:model="popBankId">
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    {{__('web_message.general.type')}}
                                </label>
                                <div class="col-sm-7">
                                    <input type="text" class="form-control" wire:model="popBankType" disabled>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    {{__('web_message.general.currency')}}
                                </label>
                                <div class="col-sm-5">
                                    <select class="form-select form-control" wire:model="popBankCurrencyId" disabled>
                                        <option value="0">
                                            {{__('web_message.general.select')}}
                                        </option>
                                        @forelse ((session()->get('pageCacheData')['currencyList']??[]) as $data)
                                        <option value="{{$data['id']}}"
                                            name="{{$data['deposit_rate']}}">{{$data['name']}}</option>
                                        @empty
                                        @endforelse
                                    </select>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">{{__('web_message.general.depBankName')}}</label>
                                <div class="col-sm-6">
                                    <input type="text" class="form-control" wire:model="popBankName" disabled>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">{{__('web_message.general.alias')}}</label>
                                <div class="col-sm-6">
                                    <input type="text" class="form-control" wire:model="modifyBankCode" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    {{__('web_message.general.minAmount')}}
                                </label>
                                <div class="col-sm-4">
                                    <input type="text" class="form-control" wire:model="modifyBankMinAmount" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}
                                        oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d\d).*$/,'$1.$2')">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    {{__('web_message.general.maxAmount')}}
                                </label>
                                <div class="col-sm-4">
                                    <input type="text" class="form-control" wire:model="modifyBankMaxAmount" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}
                                        oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d\d).*$/,'$1.$2')">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    {{__('web_message.menuMgmt.order')}}
                                </label>
                                <div class="col-sm-4">
                                    <input type="text" class="form-control" wire:model="modifyBankSort" oninput="value=value.replace(/\D/g,'')"
                                        {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    {{__('web_message.general.status')}}
                                </label>
                                <div class="col-sm-4 lh-30">
                                    <input id="modify-bank-status" class="toggle-switch" type="checkbox"
                                        wire:model="modifyBankStatus" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                    <label for="modify-bank-status"></label>
                                </div>
                            </div>
                        </div>
                        @if('modify'==(session()->get('pageCacheData')['popAction']??''))
                        <div class="panel-footer">
                            <div class="row">
                                <div class="col-sm-offset-10">
                                    <button class="btn btn-mint"
                                        type="submit">{{__('web_message.general.modify')}}</button>
                                </div>
                            </div>
                        </div>
                        @endif
                    </form>
                </div>
            </div>
        </div>
    </div>
    <style>
        .group-change {
            border: 1px solid #e9e9e9;
            border-radius: 5px;
        }

        .group-change input {
            background: #FFF !important;
            border: none;
            text-align: center;
        }

        .group-change input.fix-input {
            width: 40px !important;
        }

        .group-change select {
            border: none;
            text-align: center;
            background: #FFF !important;
        }
    </style>
    <script>
        $(function() {
            $('#depBankPopClose').click(function() {
                $('#depBankPop').removeClass('in').css('display', 'none');
                $('.modal-backdrop').css('display', 'none');
            });
        });
    </script>
</div>
@endsection