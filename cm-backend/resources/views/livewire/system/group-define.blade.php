@extends('livewire.template')
@section('content')
    <div id="page-title">
        <h1 class="page-header text-overflow">{{__('web_message.menu.system.groupDefine')}}</h1>
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
                        <label>{{__('web_message.groupMgmt.groupType')}}：</label>
                        <select class="form-select form-control" wire:model="groupType" wire:change="changeGroupType">
                            <option value="-1">{{__('web_message.general.select')}}</option>
                            @forelse ((session()->get('pageCacheData')['groupDropdown']??[]) as $data)
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
                            <button class="btn btn-purple" data-toggle="modal" data-target="#addMainDefine">
                                <i class="bi-plus-circle"></i> {{__('web_message.general.add')}} {{__('web_message.groupMgmt.mainDefine')}}
                            </button>
                        </div>
                    </div>
                </div>
                <div class="table-responsive">
                    <table class="table txt-center">
                        <thead>
                        <tr>
                            <th></th>
                            <th>{{__('web_message.groupMgmt.mainDefine')}} ID</th>
                            <th>{{__('web_message.groupMgmt.mainDefine').' '.__('web_message.general.txt')}}
                                / {{__('web_message.groupMgmt.subDefine')}} ID
                            </th>
                            <th>{{__('web_message.groupMgmt.subDefine').' '.__('web_message.general.txt')}}</th>
                            <th>{{__('web_message.groupMgmt.subDefine').' '.__('web_message.general.dataType')}}</th>
                            <th>{{__('web_message.groupMgmt.subDefine').' '.__('web_message.general.defaultValue')}}</th>
                        </tr>
                        </thead>
                        <tbody>
                        @forelse((session()->get('pageCacheData')['pageData']??[]) as $index=>$main)
                            @if(($index+1) == (session()->get('nowPage')??1))
                                <tr class="main-define">
                                    <td>
                                        <button class="btn btn-purple"
                                                wire:click="showAddSub({{$main->group_type}},{{$main->define_id}},'{{$main->name}}')"
                                                id="showAddSub">
                                            <i class="bi-plus-circle"></i> {{__('web_message.general.add')}} {{__('web_message.groupMgmt.subDefine')}}
                                        </button>
                                        <button class="btn btn-warning"
                                                wire:click="showMainPop({{$main->define_id}},'modify')">
                                            <i class="bi-pencil-square"></i>
                                        </button>
                                        <button class="btn btn-danger"
                                                wire:click="showMainPop({{$main->define_id}},'detail')">
                                            <i class="bi-list-task"></i>
                                        </button>
                                    </td>
                                    <td>{{$main->define_id}}</td>
                                    <td>{{$main->name}}</td>
                                    <td colspan="3"></td>
                                </tr>
                                @forelse(($main->sub??[]) as $sub)
                                    <tr>
                                        <td></td>
                                        <td>
                                            <button class="btn btn-warning"
                                                    wire:click="showSubPop({{$sub->define_id}},'modify')">
                                                <i class="bi-pencil-square"></i>
                                            </button>
                                            <button class="btn btn-danger"
                                                    wire:click="showSubPop({{$sub->define_id}},'detail')">
                                                <i class="bi-list-task"></i>
                                            </button>
                                        </td>
                                        <td>{{$sub->define_id}}</td>
                                        <td>{{$sub->name}}</td>
                                        <td>{{$sub->dataTypeTxt}}</td>
                                        <td>{{$sub->default_value}}</td>
                                    </tr>
                                @empty
                                @endforelse
                            @endif
                        @empty
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
                        tr.main-define td {
                            background-color: rgba(22, 174, 245, 0.3);
                        }
                    </style>
                </div>
            </div>
        </div>
        <!-- addMainDefine -->
        <div class="modal fade" id="addMainDefine" role="dialog">
            <div class="modal-dialog">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" data-dismiss="modal">&times;</button>
                        <h4 class="modal-title">{{__('web_message.general.add')}} {{__('web_message.groupMgmt.mainDefine')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal" wire:submit="addMainDefine">
                            <div class="panel-body">
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        <span
                                            class="required">*</span>{{__('web_message.groupMgmt.groupType')}}
                                    </label>
                                    <div class="col-sm-5">
                                        <select class="form-select form-control"
                                                wire:model="addMainDefineGroupType" {{-1==($addMainDefineGroupType?:-1)?'':'disabled'}}>
                                            <option value="-1">{{__('web_message.general.select')}}</option>
                                            @forelse ((session()->get('pageCacheData')['groupDropdown']??[]) as $data)
                                                <option value="{{$data->id}}">{{$data->name}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        <span class="required">*</span>{{__('web_message.groupMgmt.mainDefine')}} ID
                                    </label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" wire:model="addMainDefineId">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.language.languageSelect')}} {{__('web_message.general.txt')}}
                                    </label>
                                    <div class="col-sm-3">
                                        <select class="form-select form-control" wire:model="popDefineEnId"
                                                disabled>
                                            <option value="-1">{{__('web_message.general.select')}}</option>
                                            @forelse ((session()->get('pageCacheData')['languageDropdown']??[]) as $data)
                                                <option value="{{$data->id}}">{{$data->name}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" wire:model="addMainDefineEnName">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label"></label>
                                    <div class="col-sm-3">
                                        <select class="form-select form-control" wire:model="popDefineCnId"
                                                disabled>
                                            <option value="-1">{{__('web_message.general.select')}}</option>
                                            @forelse ((session()->get('pageCacheData')['languageDropdown']??[]) as $data)
                                                <option value="{{$data->id}}">{{$data->name}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" wire:model="addMainDefineCnName">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.general.remark')}}
                                    </label>
                                    <div class="col-sm-5">
                                        <textarea rows="5" class="form-control"
                                                  wire:model="addMainDefineRemark"></textarea>
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
        <!-- popMainDefine -->
        <div class="modal fade in" id="popMainDefine" role="dialog"
             style="display: {{'popMainDefine' == session('showPopId') ? 'block':'none'}};">
            <div class="modal-backdrop fade in" style="display: block;"></div>
            <div class="modal-dialog" style="z-index: 1050;">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" id="popMainDefineClose">&times;</button>
                        <h4 class="modal-title">{{'modify'==(session()->get('pageCacheData')['popAction']??'')?__('web_message.general.modify').' '.__('web_message.groupMgmt.mainDefine'):__('web_message.groupMgmt.mainDefine').' '.__('web_message.general.detail')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal">
                            <div class="panel-body">
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.groupMgmt.groupType')}}
                                    </label>
                                    <div class="col-sm-5">
                                        <select class="form-select form-control"
                                                wire:model="popMainGroupType" disabled>
                                            <option value="-1">{{__('web_message.general.select')}}</option>
                                            @forelse ((session()->get('pageCacheData')['groupDropdown']??[]) as $data)
                                                <option value="{{$data->id}}">{{$data->name}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.groupMgmt.mainDefine')}} ID
                                    </label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" wire:model="popMainDefineId" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.language.languageSelect')}} {{__('web_message.general.txt')}}
                                    </label>
                                    <div class="col-sm-3">
                                        <select class="form-select form-control" wire:model="popDefineEnId"
                                                disabled>
                                            <option value="-1">{{__('web_message.general.select')}}</option>
                                            @forelse ((session()->get('pageCacheData')['languageDropdown']??[]) as $data)
                                                <option value="{{$data->id}}">{{$data->name}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control"
                                               wire:model="modifyMainDefineEnName" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label"></label>
                                    <div class="col-sm-3">
                                        <select class="form-select form-control" wire:model="popDefineCnId"
                                                disabled>
                                            <option value="-1">{{__('web_message.general.select')}}</option>
                                            @forelse ((session()->get('pageCacheData')['languageDropdown']??[]) as $data)
                                                <option value="{{$data->id}}">{{$data->name}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control"
                                               wire:model="modifyMainDefineCnName" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.general.remark')}}
                                    </label>
                                    <div class="col-sm-5">
                                        <textarea rows="5" class="form-control"
                                                  wire:model="modifyMainDefineRemark"
                                        {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}></textarea>
                                    </div>
                                </div>
                            </div>
                        </form>
                    </div>
                    @if('modify'==(session()->get('pageCacheData')['popAction']??''))
                        <div class="panel-footer">
                            <div class="row">
                                <div class="col-sm-offset-10">
                                    <button class="btn btn-mint"
                                            wire:click="doModifyMainDefine">{{__('web_message.general.modify')}}</button>
                                </div>
                            </div>
                        </div>
                    @endif
                </div>
            </div>
        </div>
        <!-- addSubDefine -->
        <div class="modal fade in" id="addSubDefine" role="dialog"
             style="display: {{'addSubDefine' == session('showPopId') ? 'block':'none'}};">
            <div class="modal-backdrop fade in" style="display: block;"></div>
            <div class="modal-dialog" style="z-index: 1050;">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" id="addSubDefineClose">&times;</button>
                        <h4 class="modal-title">{{__('web_message.general.add')}} {{__('web_message.groupMgmt.subDefine')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal" wire:submit="addSubDefine">
                            <div class="panel-body">
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.groupMgmt.groupType')}}
                                    </label>
                                    <div class="col-sm-5">
                                        <select class="form-select form-control" wire:model="popSubGroupType" disabled>
                                            <option value="-1">{{__('web_message.general.select')}}</option>
                                            @forelse ((session()->get('pageCacheData')['groupDropdown']??[]) as $data)
                                                <option value="{{$data->id}}">{{$data->name}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.groupMgmt.mainDefine')}} ID
                                    </label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" wire:model="addSubDefineParentId"
                                               disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.groupMgmt.mainDefine')}}
                                    </label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" wire:model="popMainDefineName" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        <span class="required">*</span>{{__('web_message.groupMgmt.subDefine')}} ID
                                    </label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" wire:model="addSubDefineId">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.language.languageSelect')}} {{__('web_message.general.txt')}}
                                    </label>
                                    <div class="col-sm-3">
                                        <select class="form-select form-control" wire:model="popDefineEnId"
                                                disabled>
                                            <option value="-1">{{__('web_message.general.select')}}</option>
                                            @forelse ((session()->get('pageCacheData')['languageDropdown']??[]) as $data)
                                                <option value="{{$data->id}}">{{$data->name}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" wire:model="addSubDefineEnName">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label"></label>
                                    <div class="col-sm-3">
                                        <select class="form-select form-control" wire:model="popDefineCnId"
                                                disabled>
                                            <option value="-1">{{__('web_message.general.select')}}</option>
                                            @forelse ((session()->get('pageCacheData')['languageDropdown']??[]) as $data)
                                                <option value="{{$data->id}}">{{$data->name}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" wire:model="addSubDefineCnName">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        <span class="required">*</span>{{__('web_message.general.dataType')}}
                                    </label>
                                    <div class="col-sm-5">
                                        <select class="form-select form-control" wire:model="addSubDefineDataType">
                                            <option value="0">{{__('web_message.general.select')}}</option>
                                            @forelse ((session()->get('pageCacheData')['dataTypeDropdown']??[]) as $data)
                                                <option value="{{$data->code}}">{{$data->name}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        <span class="required">*</span>{{__('web_message.general.defaultValue')}}
                                    </label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" wire:model="addSubDefineDefaultValue">
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.general.remark')}}
                                    </label>
                                    <div class="col-sm-8">
                                        <textarea rows="5" class="form-control"
                                                  wire:model="addSubDefineMemo"></textarea>
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
        <!-- popSubDefine -->
        <div class="modal fade in" id="popSubDefine" role="dialog"
             style="display: {{'popSubDefine' == session('showPopId') ? 'block':'none'}};">
            <div class="modal-backdrop fade in" style="display: block;"></div>
            <div class="modal-dialog" style="z-index: 1050;">
                <div class="modal-content">
                    <div class="modal-header">
                        <button type="button" class="close" id="popSubDefineClose">&times;</button>
                        <h4 class="modal-title">{{'modify'==(session()->get('pageCacheData')['popAction']??'')?__('web_message.general.modify').' '.__('web_message.groupMgmt.subDefine'):__('web_message.groupMgmt.subDefine').' '.__('web_message.general.detail')}}</h4>
                    </div>
                    <div class="modal-body">
                        <form class="form-horizontal">
                            <div class="panel-body">
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.groupMgmt.groupType')}}
                                    </label>
                                    <div class="col-sm-5">
                                        <select class="form-select form-control" wire:model="popSubGroupType" disabled>
                                            <option value="-1">{{__('web_message.general.select')}}</option>
                                            @forelse ((session()->get('pageCacheData')['groupDropdown']??[]) as $data)
                                                <option value="{{$data->id}}">{{$data->name}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.groupMgmt.mainDefine')}} ID
                                    </label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" wire:model="popSubDefineParentId"
                                               disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.groupMgmt.mainDefine')}}
                                    </label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" wire:model="popMainDefineName" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.groupMgmt.subDefine')}} ID
                                    </label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control" wire:model="modifySubDefineId" disabled>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.language.languageSelect')}} {{__('web_message.general.txt')}}
                                    </label>
                                    <div class="col-sm-3">
                                        <select class="form-select form-control" wire:model="popDefineEnId"
                                                disabled>
                                            <option value="-1">{{__('web_message.general.select')}}</option>
                                            @forelse ((session()->get('pageCacheData')['languageDropdown']??[]) as $data)
                                                <option value="{{$data->id}}">{{$data->name}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control"
                                               wire:model="modifySubDefineEnName" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label"></label>
                                    <div class="col-sm-3">
                                        <select class="form-select form-control" wire:model="popDefineCnId"
                                                disabled>
                                            <option value="-1">{{__('web_message.general.select')}}</option>
                                            @forelse ((session()->get('pageCacheData')['languageDropdown']??[]) as $data)
                                                <option value="{{$data->id}}">{{$data->name}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control"
                                               wire:model="modifySubDefineCnName" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.general.dataType')}}
                                    </label>
                                    <div class="col-sm-5">
                                        <select class="form-select form-control"
                                                wire:model="popSubDefineDataType" disabled>
                                            <option value="-1">{{__('web_message.general.select')}}</option>
                                            @forelse ((session()->get('pageCacheData')['dataTypeDropdown']??[]) as $data)
                                                <option value="{{$data->code}}">{{$data->name}}</option>
                                            @empty
                                            @endforelse
                                        </select>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.general.defaultValue')}}
                                    </label>
                                    <div class="col-sm-5">
                                        <input type="text" class="form-control"
                                               wire:model="modifySubDefineDefaultValue" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label class="col-sm-3 control-label">
                                        {{__('web_message.general.remark')}}
                                    </label>
                                    <div class="col-sm-8">
                                        <textarea rows="5" class="form-control"
                                                  wire:model="modifySubDefineMemo" {{'modify'==(session()->get('pageCacheData')['popAction']??'')?'':'disabled'}}></textarea>
                                    </div>
                                </div>
                            </div>
                        </form>
                    </div>
                    @if('modify'==(session()->get('pageCacheData')['popAction']??''))
                        <div class="panel-footer">
                            <div class="row">
                                <div class="col-sm-offset-10">
                                    <button class="btn btn-mint"
                                            wire:click="doModifySubDefine">{{__('web_message.general.modify')}}</button>
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
                $('#popMainDefineClose').click(function () {
                    $('#popMainDefine').removeClass('in').css('display', 'none');
                    $('.modal-backdrop').css('display', 'none');
                });
                $('#addSubDefineClose').click(function () {
                    $('#addSubDefine').removeClass('in').css('display', 'none');
                    $('.modal-backdrop').css('display', 'none');
                });
                $('#popSubDefineClose').click(function () {
                    $('#popSubDefine').removeClass('in').css('display', 'none');
                    $('.modal-backdrop').css('display', 'none');
                });
            });
        </script>
    </div>
@endsection
