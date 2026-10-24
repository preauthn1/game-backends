@extends('livewire.template')
@section('content')
<div id="page-title">
    <h1 class="page-header text-overflow">{{__('web_message.menu.system.withdrawalSetting')}}</h1>
</div>
<div id="page-content">
    <!--设置USDT to 法币汇率-->
    <div class="panel">
        <div class="panel-heading">
            <h3 class="panel-title">{{__('web_message.general.setting')}} USDT
                to {{__('web_message.general.faitMoney')}} {{__('web_message.general.fxRate')}}</h3>
        </div>
        <div class="panel-body">
            <form class="form-inline" wire:submit="updateWithdrawalRate" style="position: relative;">
                <div class="form-group group-change">
                    <input type="text" class="form-control fix-input" value="1" disabled>
                    <select class="form-select form-control" disabled>
                        <option selected>USDT</option>
                    </select>
                </div>
                <div class="form-group"><i class="bi-arrow-left-right" style="margin: 0 10px;"></i></div>
                <div class="form-group group-change">
                    <input type="text" placeholder="{{__('web_message.general.fxRate')}}" class="form-control"
                        wire:model="fxRate" style="width: 140px;"
                        oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d\d\d\d\d\d\d\d\d).*$/,'$1.$2')">
                    <select class="form-select form-control" wire:model="rateCurrencyId" wire:change="changeRate">
                        <option value="0">
                            {{__('web_message.general.select')}} {{__('web_message.general.currency')}}
                        </option>
                        @forelse ((session()->get('pageCacheData')['currencyList']??[]) as $data)
                        <option value="{{$data['id']}}"
                            name="{{$data['withdrawal_rate']}}">{{$data['name']}}</option>
                        @empty
                        @endforelse
                    </select>
                </div>
                <button class="btn btn-primary btn-search mx-3"
                    type="submit">{{__('web_message.general.setting')}}
                </button>
            </form>
        </div>
    </div>
    <!--设置币种提现手续费比率-->
    <div class="panel">
        <div class="panel-heading">
            <h3 class="panel-title">{{__('web_message.general.setting')}} {{__('web_message.general.currency')}} {{__('web_message.general.withdrawalFee')}}</h3>
        </div>
        <div class="panel-body">
            <form class="form-inline" wire:submit="updateCurrencyWithdrawalFeeRate" style="position: relative;">
                <div class="form-group">
                    <select class="form-select form-control" wire:model="feeCurrencyId">
                        <option value="0">
                            {{__('web_message.general.select')}} {{__('web_message.general.currency')}}
                        </option>
                        @forelse ((session()->get('pageCacheData')['currencyList']??[]) as $data)
                        <option value="{{$data['id']}}">{{$data['name']}}</option>
                        @empty
                        @endforelse
                    </select>
                </div>
                <div class="form-group">
                    <select class="form-select form-control" wire:model="currencyFeeType"
                        wire:change="changeFeeType">
                        @forelse ((session()->get('pageCacheData')['feeTypeList']??[]) as $data)
                        <option value="{{$data->id}}">{{$data->name}}</option>
                        @empty
                        @endforelse
                    </select>
                </div>
                <div class="form-group">
                    <input type="text" placeholder="{{(1==$currencyFeeType)?__('web_message.general.amount'):__('web_message.general.rate')}}" class="form-control"
                        wire:model="currencyFeeNumber" style="width: 100px;text-align: center;"
                        oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')">
                    @if(1==$currencyFeeType)
                    <span>USDT</span>
                    @else
                    <span>%</span>
                    @endif
                </div>
                <button class="btn btn-primary btn-search mx-3"
                    type="submit">{{__('web_message.general.setting')}}
                </button>
            </form>
        </div>
    </div>
    <!--提現渠道 搜索區塊-->
    <div class="panel">
        <div class="panel-heading">
            <h3 class="panel-title">{{__('web_message.withdrawalSetting.wdBankSetting')}}</h3>
        </div>
        <div class="panel-body">
            <form class="form-inline" wire:submit="doSearch" style="position: relative;">
                <div class="form-group">
                    <label>{{__('web_message.general.type')}}：</label>
                    <select class="form-select form-control" wire:model="channelType">
                        <option value="0">
                            {{__('web_message.general.select')}}
                        </option>
                        @forelse ((session()->get('pageCacheData')['channelList']??[]) as $data)
                        <option value="{{$data->id}}">{{$data->name}}</option>
                        @empty
                        @endforelse
                    </select>
                </div>
                <div class="form-group">
                    <label>{{__('web_message.general.currency')}}：</label>
                    <select class="form-select form-control" wire:model="channelCurrency">
                        <option value="0">
                            {{__('web_message.general.select')}}
                        </option>
                        @forelse ((session()->get('pageCacheData')['currencyList']??[]) as $data)
                        <option value="{{$data['id']}}">{{$data['name']}}</option>
                        @empty
                        @endforelse
                    </select>
                </div>
                <div class="form-group">
                    <label>{{__('web_message.general.alias')}}：</label>
                    <input type="text" class="form-control" wire:model="channelAlias">
                </div>
                <button class="btn btn-primary" type="submit">{{__('web_message.general.search')}}</button>
            </form>
        </div>
    </div>
    <!--提現渠道 資料區塊-->
    <div class="panel">
        <div class="panel-body">
            <div class="pad-btm form-inline">
                <div class="row">
                    <div class="col-sm-6 table-toolbar-left">
                        <button class="btn btn-purple" data-toggle="modal" data-target="#addWdBank">
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
                            <th>{{__('web_message.general.bank')}}</th>
                            <th>{{__('web_message.general.alias')}}</th>
                            <th>{{__('web_message.general.image')}}</th>
                            <th>{{__('web_message.general.minAmount')}}</th>
                            <th>{{__('web_message.general.maxAmount')}}</th>
                            <th>{{__('web_message.withdrawalSetting.feeType')}}</th>
                            <th>{{__('web_message.general.amount').'/'.__('web_message.general.rate')}}</th>
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
                            <td>{{$data->id}}</td>
                            <td style="font-size: 16px;">
                                <i class="{{(1==$data->status)?'bi-toggle-on':'bi-toggle-off'}}"></i>
                            </td>
                            <td>{{$data->bankTypeTxt}}</td>
                            <td>{{$data->currency_name}}</td>
                            <td>{{$data->bank_name??''}}</td>
                            <td>{{$data->account_alias??''}}</td>
                            <td>
                                <img src="{{ $data->image }}"
                                    onclick="window.open('{{ $data->image }}','_blank');"
                                    style="max-height: 32px; width:60px; object-fit: contain; cursor: pointer;" />
                            </td>
                            <td>{{$data->min_amount}}</td>
                            <td>{{$data->max_amount}}</td>
                            <td>{{$data->taxTypeTxt}}</td>
                            <td>{{$data->taxTxt}}</td>
                            <td>{{$data->sort??''}}</td>
                        </tr>
                        @empty
                        <tr>
                            <td colspan="13">{{__('web_message.general.noData')}}</td>
                        </tr>
                        @endforelse
                    </tbody>
                    <tfoot>
                        <tr>
                            <td colspan="13" class="footable-visible">
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
    <!-- addWdBank -->
    <div class="modal fade" id="addWdBank" role="dialog">
        <div class="modal-dialog" style="margin-top: 30px;">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal" id="addWdBankClose">&times;</button>
                    <h4 class="modal-title">{{__('web_message.withdrawalSetting.addWdBank')}}</h4>
                </div>
                <div class="modal-body">
                    <form class="form-horizontal" wire:submit="addWdBank">
                        <div class="panel-body">
                            <div class="form-group">
                                <label class="col-sm-3 control-label">
                                    <span class="required">*</span>{{__('web_message.general.type')}}
                                </label>
                                <div class="col-sm-8">
                                    <select class="form-select form-control" wire:model="addTypeId">
                                        <option value="0">
                                            {{__('web_message.general.select')}}
                                        </option>
                                        @forelse ((session()->get('pageCacheData')['channelList']??[]) as $data)
                                        <option value="{{$data->id}}">{{$data->name}}</option>
                                        @empty
                                        @endforelse
                                    </select>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-3 control-label">
                                    <span class="required">*</span>{{__('web_message.general.currency')}}
                                </label>
                                <div class="col-sm-5">
                                    <select class="form-select form-control" wire:model="addCurrencyId">
                                        <option value="0">
                                            {{__('web_message.general.select')}}
                                        </option>
                                        @forelse ((session()->get('pageCacheData')['currencyList']??[]) as $data)
                                        <option value="{{$data['id']}}">{{$data['name']}}</option>
                                        @empty
                                        @endforelse
                                    </select>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-3 control-label">
                                    <span class="required">*</span>{{__('web_message.general.bank')}}
                                </label>
                                <div class="col-sm-5">
                                    <input type="text" class="form-control" wire:model="addBankName">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-3 control-label">
                                    <span class="required">*</span>{{__('web_message.general.alias')}}
                                </label>
                                <div class="col-sm-5">
                                    <input type="text" class="form-control" wire:model="addAlias">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-3 control-label">
                                    {{__('web_message.general.image')}}
                                </label>
                                <div class="col-sm-6">
                                    <ul class="mail-attach-list list-ov">
                                        <li>
                                            <div class="mail-file-img" style="border: 1px solid #ddd;">
                                                <img class="image-responsive" id="addPreviewImage"
                                                    style="width: 120px;">
                                            </div>
                                        </li>
                                    </ul>
                                    <input type="file" class="form-control" accept="image/*" id="addSelectImage">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-3 control-label">
                                    <span class="required">*</span>{{__('web_message.general.minAmount')}}
                                </label>
                                <div class="col-sm-5">
                                    <input type="text" class="form-control" wire:model="addMinAmount"
                                        oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-3 control-label">
                                    <span class="required">*</span>{{__('web_message.general.maxAmount')}}
                                </label>
                                <div class="col-sm-5">
                                    <input type="text" class="form-control" wire:model="addMaxAmount"
                                        oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-3 control-label">
                                    {{__('web_message.withdrawalSetting.feeType')}}
                                </label>
                                <div class="col-sm-5">
                                    <select class="form-select form-control" wire:model="addTaxType">
                                        <option value="-1">{{__('web_message.general.select')}}</option>
                                        @forelse((session()->get('pageCacheData')['feeTypeList']??[]) as $data)
                                        <option value="{{$data->id}}">{{$data->name}}</option>
                                        @empty
                                        @endforelse
                                    </select>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-3 control-label">
                                    {{__('web_message.general.amount').'/'.__('web_message.general.rate')}}
                                </label>
                                <div class="col-sm-5">
                                    <input type="text" class="form-control" wire:model="addTax"
                                        oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-3 control-label">{{__('web_message.menuMgmt.order')}}</label>
                                <div class="col-sm-4">
                                    <input type="text" class="form-control" wire:model="addOrder" oninput="value=value.replace(/\D/g,'')">
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
    <!-- wdBankPop -->
    <div class="modal fade in" id="wdBankPop" role="dialog"
        style="display: {{'wdBankPop' == session('showPopId') ? 'block':'none'}};">
        <div class="modal-backdrop fade in" style="display: block;"></div>
        <div class="modal-dialog" style="z-index: 1050;">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" id="wdBankPopClose">&times;</button>
                    <h4 class="modal-title">{{'modify'==(session()->get('pageCacheData')['popAction']??'')?__('web_message.withdrawalSetting.modifyWdBank'):__('web_message.withdrawalSetting.wdBankDetail')}}</h4>
                </div>
                <div class="modal-body">
                    <form class="form-horizontal" wire:submit="doModify">
                        <div class="panel-body">
                            <input type="hidden" wire:model="modifyBankId">
                            <div class="form-group">
                                <label class="col-sm-3 control-label">
                                    {{__('web_message.general.type')}}
                                </label>
                                <div class="col-sm-8">
                                    <input type="text" class="form-control" wire:model="popBankTypeTxt"
                                        disabled>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-3 control-label">
                                    {{__('web_message.general.currency')}}
                                </label>
                                <div class="col-sm-5">
                                    <input type="text" class="form-control" wire:model="popCurrencyName"
                                        disabled>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-3 control-label">
                                    {{__('web_message.general.bank')}}
                                </label>
                                <div class="col-sm-5">
                                    <input type="text" class="form-control" wire:model="popBankName"
                                        disabled>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-3 control-label">
                                    {{__('web_message.general.alias')}}
                                </label>
                                <div class="col-sm-5">
                                    <input type="text" class="form-control" wire:model="modifyBankAlias"
                                        {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-3 control-label">{{__('web_message.general.image')}}</label>
                                <div class="col-sm-6">
                                    <ul class="mail-attach-list list-ov">
                                        <li>
                                            <div class="mail-file-img" style="border: 1px solid #ddd;">
                                                <img class="image-responsive" src="{{$modifyImageUrl}}"
                                                    style="width: 120px;cursor: pointer;" id="popPreviewImage"
                                                    onclick="window.open('{{$modifyImageUrl}}','_blank');">
                                            </div>
                                        </li>
                                    </ul>
                                    <input type="file" class="form-control" id="popSelectImage"
                                        accept="image/*"
                                        style="display: {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'block':'none'}};">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-3 control-label">
                                    {{__('web_message.general.minAmount')}}
                                </label>
                                <div class="col-sm-5">
                                    <input type="text" class="form-control" wire:model="modifyMinAmount"
                                        oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')"
                                        {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-3 control-label">
                                    {{__('web_message.general.maxAmount')}}
                                </label>
                                <div class="col-sm-5">
                                    <input type="text" class="form-control" wire:model="modifyMaxAmount"
                                        oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')"
                                        {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-3 control-label">
                                    {{__('web_message.withdrawalSetting.feeType')}}
                                </label>
                                <div class="col-sm-5">
                                    <select class="form-select form-control" wire:model="modifyTaxType"
                                        {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                        <option value="-1">{{__('web_message.general.select')}}</option>
                                        @forelse((session()->get('pageCacheData')['feeTypeList']??[]) as $data)
                                        <option value="{{$data->id}}">{{$data->name}}</option>
                                        @empty
                                        @endforelse
                                    </select>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-3 control-label">
                                    {{__('web_message.general.amount').'/'.__('web_message.general.rate')}}
                                </label>
                                <div class="col-sm-5">
                                    <input type="text" class="form-control" wire:model="modifyTax"
                                        oninput="value=value.replace(/[^\d.]/g,'').replace(/^(\d+)\.(\d\d).*$/,'$1.$2')"
                                        {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-3 control-label">
                                    {{__('web_message.menuMgmt.order')}}
                                </label>
                                <div class="col-sm-4">
                                    <input type="text" class="form-control" wire:model="modifySort" oninput="value=value.replace(/\D/g,'')"
                                        {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-3 control-label">
                                    {{__('web_message.general.status')}}
                                </label>
                                <div class="col-sm-4 lh-30">
                                    <input id="modify-bank-status" class="toggle-switch" type="checkbox"
                                        wire:model="modifyStatus" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
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
    <!--頁面特殊JS-->
    <script>
        $(function() {
            $('#addWdBankClose').click(function() {
                $('#addSelectImage').val('');
                $('#addPreviewImage').attr('src', '');
            });
            $('#wdBankPopClose').click(function() {
                $('#wdBankPop').removeClass('in').css('display', 'none');
                $('.modal-backdrop').css('display', 'none');
                $('#popSelectImage').val('');
            });

            function addMinerReadFile() {
                if (!this.files || !this.files[0]) return;
                const FR = new FileReader();
                FR.addEventListener("load", function(evt) {
                    document.querySelector("#addPreviewImage").src = evt.target.result;
                    Livewire.dispatch('addImageUrl', {
                        data: evt.target.result
                    });
                    //document.querySelector("#base64Image").textContent = evt.target.result;
                });
                FR.readAsDataURL(this.files[0]);
            }
            document.querySelector("#addSelectImage").addEventListener("change", addMinerReadFile);

            function popImageReadFile() {
                if (!this.files || !this.files[0]) return;
                const FR = new FileReader();
                FR.addEventListener("load", function(evt) {
                    document.querySelector("#popPreviewImage").src = evt.target.result;
                    Livewire.dispatch('modifyImageUrl', {
                        data: evt.target.result
                    });
                    //document.querySelector("#base64Image").textContent = evt.target.result;
                });
                FR.readAsDataURL(this.files[0]);
            }
            document.querySelector("#popSelectImage").addEventListener("change", popImageReadFile);
        });
    </script>
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
</div>
@endsection