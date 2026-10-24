@extends('livewire.template')
@section('content')
<div id="page-title">
    <h1 class="page-header text-overflow">{{__('web_message.menu.system.depositAccountSetting')}}</h1>
</div>
<div id="page-content">
    <!--收款帳號設置 搜索區塊-->
    <div class="panel">
        <div class="panel-heading">
            <h3 class="panel-title">{{__('web_message.depositSetting.depAccountSetting')}}</h3>
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
                <div class="form-group">
                    <label>{{__('web_message.general.status')}}：</label>
                    <select class="form-select form-control" wire:model="channelStatus">
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
    <!--收款帳號設置 資料區塊-->
    <div class="panel">
        <div class="panel-body">
            <div class="pad-btm form-inline">
                <div class="row">
                    <div class="col-sm-6 table-toolbar-left">
                        <button class="btn btn-purple" wire:click="showAddChannel()">
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
                            <th>{{__('web_message.general.alias')}}</th>
                            <th>{{__('web_message.general.depBankName')}}</th>
                            <th>{{__('web_message.general.depAccountName')}}</th>
                            <th>{{__('web_message.general.depAccountNo')}}</th>
                        </tr>
                    </thead>
                    <tbody>
                        @forelse((session()->get('pageCacheData')['pageData']??[]) as $data)
                        <tr>
                            <td>
                                @if(1==$data->status)
                                <button class="btn btn-danger" wire:click="doModifyStatus({{$data->id}},0)">
                                    {{__('param.disable')}}
                                </button>
                                @else
                                <button class="btn btn-warning" wire:click="doModifyStatus({{$data->id}},1)">
                                    {{__('param.enable')}}
                                </button>
                                @endif
                            </td>
                            <td>{{$data->id??''}}</td>
                            <td style="font-size: 16px;">
                                <i class="{{(1==$data->status)?'bi-toggle-on':'bi-toggle-off'}}"></i>
                            </td>
                            <td>{{$data->gatewayTxt??''}}</td>
                            <td>{{$data->currency_name??''}}</td>
                            <td>{{$data->account_nickname??''}}</td>
                            <td>{{$data->bank_name??''}}</td>
                            <td>{{$data->account_name??''}}</td>
                            <td>{{$data->account_no??''}}</td>
                        </tr>
                        @empty
                        <tr>
                            <td colspan="9">{{__('web_message.general.noData')}}</td>
                        </tr>
                        @endforelse
                    </tbody>
                    <tfoot>
                        <tr>
                            <td colspan="9" class="footable-visible">
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
    <!-- 新增收款帳號 -->
    <div class="modal fade in" id="addChannel" role="dialog"
        style="display: {{'addChannel' == session('showPopId') ? 'block':'none'}};">
        <div class="modal-backdrop fade in" style="display: block;"></div>
        <div class="modal-dialog" style="z-index: 1050;">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" id="addClose">&times;</button>
                    <h4 class="modal-title">{{__('web_message.depositSetting.addDepAccount')}}</h4>
                </div>
                <div class="modal-body">
                    <form class="form-horizontal" wire:submit="addChannel">
                        <div class="panel-body">
                            <div class="form-group">
                                <label class="col-sm-5 control-label">
                                    <span class="required">*</span>{{__('web_message.general.currency')}}
                                </label>
                                <div class="col-sm-5">
                                    <select class="form-select form-control" wire:model="addChannelCurrencyId"
                                        wire:change="changeBank">
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
                                <label class="col-sm-5 control-label">
                                    <span class="required">*</span>{{__('web_message.general.depBankName')}}
                                </label>
                                <div class="col-sm-5">
                                    <select class="form-select form-control" wire:model="addChannelBankId">
                                        <option value="0">
                                            {{__('web_message.general.select')}}
                                        </option>
                                        @forelse ((session()->get('pageCacheData')['bankList']??[]) as $data)
                                        <option value="{{$data['bank_id']}}">{{$data['name']}}</option>
                                        @empty
                                        @endforelse
                                    </select>
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-5 control-label">
                                    {{__('web_message.general.alias')}}
                                </label>
                                <div class="col-sm-5">
                                    <input type="text" class="form-control" wire:model="addChannelAlias">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-5 control-label">
                                    <span class="required">*</span>{{__('web_message.general.depAccountName')}}
                                </label>
                                <div class="col-sm-5">
                                    <input type="text" class="form-control" wire:model="addChannelAccountName">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-5 control-label">
                                    <span class="required">*</span>{{__('web_message.general.depAccountNo')}}
                                </label>
                                <div class="col-sm-5">
                                    <input type="text" class="form-control" wire:model="addAccountNo">
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
    <script>
        $(function() {
            $('#addClose').click(function() {
                $('#addChannel').removeClass('in').css('display', 'none');
                $('.modal-backdrop').css('display', 'none');
            });
        });
    </script>
</div>
@endsection