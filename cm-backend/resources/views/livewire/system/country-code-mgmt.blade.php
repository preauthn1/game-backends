@extends('livewire.template')
@section('content')
<div id="page-title">
    <h1 class="page-header text-overflow">{{__('web_message.menu.system.countryCodeMgmt')}}</h1>
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
                    <label>{{__('web_message.countryCodeMgmt.isoCode')}}：</label>
                    <input type="text" class="form-control" wire:model="isoCode">
                </div>
                <div class="form-group">
                    <label>{{__('web_message.countryCodeMgmt.countryCode')}}：</label>
                    <input type="text" class="form-control" wire:model="countryCode">
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
                        <button class="btn btn-purple" data-toggle="modal" data-target="#addCountry">
                            <i class="bi-plus-circle"></i> {{__('web_message.general.add')}}
                        </button>
                    </div>
                </div>
            </div>
            <div class="table-responsive">
                <table class="table txt-center">
                    <thead>
                        <tr>
                            <th></th>
                            <th>{{__('web_message.general.status')}}</th>
                            <th>ID</th>
                            <th>{{__('web_message.countryCodeMgmt.countryCode')}}</th>
                            <th>{{__('web_message.countryCodeMgmt.isoCode')}}</th>
                            <th>{{__('web_message.countryCodeMgmt.countryName')}}</th>
                            <th>{{__('web_message.countryCodeMgmt.phoneRule')}}</th>
                            <th>{{__('web_message.menuMgmt.order')}}</th>
                        </tr>
                    </thead>
                    <tbody>
                        @forelse((session()->get('pageCacheData')['pageData']??[]) as $seq=>$data)
                        <tr>
                            <td>
                                <button class="btn btn-warning" wire:click="showModify({{$data->id}})">
                                    <i class="bi-pencil-square"></i>
                                </button>
                            </td>
                            <td style="font-size: 16px;">
                                <i class="{{(1==$data->status)?'bi-toggle-on':'bi-toggle-off'}}"></i>
                            </td>
                            <td>{{ $data->id }}</td>
                            <td>{{ $data->country_code }}</td>
                            <td>{{ $data->iso_code }}</td>
                            <td>{{ $data->country_name }}</td>
                            <td>{{ $data->phone_rule }}</td>
                            <td>{{ $data->sort }}</td>
                        </tr>
                        @empty
                        <tr>
                            <td colspan="8">{{__('web_message.general.noData')}}</td>
                        </tr>
                        @endforelse
                    </tbody>
                    <tfoot>
                        <tr>
                            <td colspan="8" class="footable-visible">
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
    <!-- addCountry -->
    <div class="modal fade" id="addCountry" role="dialog">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" data-dismiss="modal">&times;</button>
                    <h4 class="modal-title">{{__('web_message.menu.system.addCountryCode')}}</h4>
                </div>
                <div class="modal-body">
                    <form class="form-horizontal" wire:submit="addCountry">
                        <div class="panel-body">
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    <span class="required">*</span>{{__('web_message.countryCodeMgmt.countryCode')}}
                                </label>
                                <div class="col-sm-4">
                                    <input type="text" class="form-control" wire:model="addCountryCode" placeholder="229">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    <span class="required">*</span>{{__('web_message.countryCodeMgmt.isoCode')}}
                                </label>
                                <div class="col-sm-3">
                                    <input type="text" class="form-control" wire:model="addIsoCode" placeholder="BJ">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    <span class="required">*</span>{{__('web_message.countryCodeMgmt.countryName')}}
                                </label>
                                <div class="col-sm-6">
                                    <input type="text" class="form-control" wire:model="addCountryName" placeholder="Benin">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    <span class="required">*</span>{{__('web_message.countryCodeMgmt.phoneRule')}}
                                </label>
                                <div class="col-sm-6">
                                    <input type="text" class="form-control" wire:model="addPhoneRule" placeholder="^[0-9]\d{10}$">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    {{__('web_message.menuMgmt.order')}}
                                </label>
                                <div class="col-sm-3">
                                    <input type="text" class="form-control" wire:model="addOrder">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    <span class="required">*</span>{{__('web_message.general.status')}}
                                </label>
                                <div class="col-sm-4 lh-30">
                                    <input id="add-status" class="toggle-switch" type="checkbox"
                                        wire:model="addStatus">
                                    <label for="add-status"></label>
                                </div>
                            </div>
                        </div>
                        <div class="panel-footer">
                            <div class="row">
                                <div class="col-sm-offset-10">
                                    <a class="btn btn-default" target="_blank" style="position: absolute;left: 5%;"
                                        href="https://example.invalid/">
                                        {{__('web_message.general.references')}}
                                    </a>
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
    <!-- modifyCountry -->
    <div class="modal fade in" id="modifyCountry" role="dialog"
        style="display: {{'modifyCountry' == session('showPopId') ? 'block':'none'}};">
        <div class="modal-backdrop fade in" style="display: block;"></div>
        <div class="modal-dialog" style="z-index: 1050;">
            <div class="modal-content">
                <div class="modal-header">
                    <button type="button" class="close" id="modifyClose">&times;</button>
                    <h4 class="modal-title">{{__('web_message.menu.system.modifyCountryCode')}}</h4>
                </div>
                <div class="modal-body">
                    <form class="form-horizontal" wire:submit="doModify">
                        <div class="panel-body">
                            <input type="hidden" wire:model="modifyId">
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    {{__('web_message.countryCodeMgmt.countryCode')}}
                                </label>
                                <div class="col-sm-4">
                                    <input type="text" class="form-control" wire:model="modifyCountryCode">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    {{__('web_message.countryCodeMgmt.isoCode')}}
                                </label>
                                <div class="col-sm-3">
                                    <input type="text" class="form-control" wire:model="modifyIsoCode">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    {{__('web_message.countryCodeMgmt.countryName')}}
                                </label>
                                <div class="col-sm-6">
                                    <input type="text" class="form-control" wire:model="modifyCountryName">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    {{__('web_message.countryCodeMgmt.phoneRule')}}
                                </label>
                                <div class="col-sm-6">
                                    <input type="text" class="form-control" wire:model="modifyPhoneRule">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    {{__('web_message.menuMgmt.order')}}
                                </label>
                                <div class="col-sm-3">
                                    <input type="text" class="form-control" wire:model="modifyOrder">
                                </div>
                            </div>
                            <div class="form-group">
                                <label class="col-sm-4 control-label">
                                    {{__('web_message.general.status')}}
                                </label>
                                <div class="col-sm-4 lh-30">
                                    <input id="modify-status" class="toggle-switch" type="checkbox"
                                        wire:model="modifyStatus">
                                    <label for="modify-status"></label>
                                </div>
                            </div>
                        </div>
                        <div class="panel-footer">
                            <div class="row">
                                <div class="col-sm-offset-10">
                                    <button class="btn btn-mint"
                                        type="submit">{{__('web_message.general.modify')}}</button>
                                </div>
                            </div>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    </div>
    <!--頁面特殊JS-->
    <script>
        $(function() {
            $('#modifyClose').click(function() {
                $('#modifyCountry').removeClass('in').css('display', 'none');
                $('.modal-backdrop').css('display', 'none');
            });
        });
    </script>
</div>
@endsection