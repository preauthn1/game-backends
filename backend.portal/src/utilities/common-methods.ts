import XLSX2 from 'xlsx'; // SheetJS js-xlsx
import * as XLSX from 'xlsx-js-style';
import EventBus from '@/utilities/event-bus';
import SimpleUpdateApi from '@/api/commons/simpleUpdate';
import * as DropdownModel from '@/models/interfaces/commons/dropdown';
import { datetimeMixin, startEndDate, timeWithTimezone, daysAgo, weeksAgo, monthsAgo, hourAgo, formatMixin } from '@/utilities/datetime-format';


export const commonMethodMixin = {


	data () {
		return {			
			globalKeywordSearchGamesDropdown: '',
			globalKeywordSearchMerchantsDropdown: '',		
			staticResourceUrl: 'https://example.invalid',		
		};
	},


	methods: {		

		getBackofficeUrl()
		{
			return window.location.origin;
		},

		getDefaultPassword()
		{
			return 'abc123';
		},

		getDefaultPasswordV2026()
		{
			return 'SYS_DEFAULT_PSW';
		},

		expandMaxWidth(event: any)
		{

			console.log(event);
			console.log(event.target);


			// 移除掉限制寬度的cssClass
			event.target.classList.remove("maxWidth100");
			event.target.classList.remove("maxWidth150");
			event.target.classList.remove("maxWidth200");
			event.target.classList.remove("maxWidth250");
			event.target.classList.remove("maxWidth300");
			event.target.classList.remove("maxWidth400");
			event.target.classList.remove("maxWidth500");
		},
	
		visibleOrNot(isVisible: boolean, columnName: string) {
			if (!isVisible) {
				return 'invisibleColumn';
			}
			return columnName;
		},
	
		visibleOrDashValue(isVisible: boolean, myValue: any) {
			if (!isVisible) {
				return '-';
			}
			return myValue;
		},

		copySuccessMsg() {
			EventBus.$emit(`successfully-copy`);
		},

		isDevEnv()
		{
			const env = process.env.VUE_APP_HOST;		
	
			if (env && !(env === undefined)) {
				if (env === 'development') {
					return true;
				} else if (env === 'local') {				
					return true;
				} else if (env === 'qa') {					
					return false;
				} else {
					return false;
				}
			} else {				
				return false;
			}			
		},

		isEmptyCollection(list : any[]) {
			if(list == null) {
				return true;
			}

			if(list) {

				if(list.length > 0) {
					return false;
				}
			}

			return true;
		},

		isEmptyString(text : string) {
			if(text === undefined) {
				return true;
			}

			if(text === null) {
				return true;
			}

			if(text === '') {
				return true;
			}

			return false;
		},

		resetDropdwonKeyword()
		{
			const vm: any = this;
			vm.globalKeywordSearchGamesDropdown = '';
			vm.globalKeywordSearchMerchantsDropdown = '';
		},

		// 如果使用到這個通用function (請在按搜尋按紐的時候 搭配  vm.globalKeywordSearchGamesDropdown = '' 清空關鍵字 不然大概又有奇怪bug
		filterDropdownItemsByKeywordSearch(dropdownItems: any[] , keyword: string , currentSelectedValue: number)
		{			
			// v-auto-complete 還要加個 no-filter 
			// 需要自訂template selection (很重要不然會無窮迴圈)
			const vm: any = this;

			if (vm.isEnableSimplifyDropdown()) {
				if (keyword != null && keyword.length > 0) {
					keyword = keyword.toLowerCase();
					const resultItems = dropdownItems.filter((z) => z.fullTextForKeywordSearch.includes(keyword) || z.valueInt === currentSelectedValue);							
					return resultItems;
				} 
				else if(currentSelectedValue > 0)
				{			
					console.log(currentSelectedValue);	

					const resultItems = dropdownItems.filter((z) => z.valueInt === currentSelectedValue);							
					return resultItems;
				}		
				else {
					return [];
				}
			} else {
				if (keyword != null && keyword.length > 0) {
					keyword = keyword.toLowerCase();
					// 當前選擇的也search出來 避免某些bug (客戶的神祕操作)
					const resultItems = dropdownItems.filter((z) => z.fullTextForKeywordSearch.includes(keyword) || z.valueInt === currentSelectedValue);					
					return resultItems;

				} else {
					return dropdownItems;
				}
			}
		},

		// 強制關掉某個頁面的導覽列Tab (PageHeader.vue)
		forceClosePageMenuByPath(routerPath : string)
		{
			// @ts-ignore
			const targetElementID = '#menu_' + routerPath.replaceAll('/' , '_');
			const targetElement : any = document.querySelector(targetElementID + ' .v-chip__close');

			if(targetElement)
			{
				targetElement.click();
			}
		},


		// 使用這支需要import PopupCopyDatetime from '@/components/PopupCopyDatetime.vue'; 並且注入該component
		globalPopupCopyDateTime(datetimeUTC : string , myIdentityText :string = '')
		{
			const vm : any = this;
			vm.$refs.refCopyDateTime.setDateTimeAndPopup(datetimeUTC , myIdentityText);
			
		},

		globalClear() {			
			const vm : any = this;


			if(vm.datetimeRangePicker)
			{
				vm.datetimeRangePicker.quicklyPickTimeRangeText = '';
				vm.datetimeRangePicker.searchStartTimeRange = [];
			}
		},

		globalQuicklyPickTimeRange(text: string) {

			const $this : any = this;

			// 若要呼叫此function 需要確保有 datetimeRangePicker 這個變數定義在class中
			// 範例: datetimeRangePicker: DropdownModel.DatetimeRangePicker = new DropdownModel.DatetimeRangePicker();

			$this.datetimeRangePicker.quicklyPickTimeRangeText = text;

			switch (text) {
				case 'today':
					$this.datetimeRangePicker.searchStartTimeRange = [daysAgo(0), daysAgo(1)];
					break;
	
				case 'yesterday':
					$this.datetimeRangePicker.searchStartTimeRange = [daysAgo(-1), daysAgo(0)];
					break;
	
				case 'thisWeek':
					$this.datetimeRangePicker.searchStartTimeRange = weeksAgo(0);
					break;
	
				case 'lastWeek':
					$this.datetimeRangePicker.searchStartTimeRange = weeksAgo(1);
					break;
	
				case 'thisMonth':
					$this.datetimeRangePicker.searchStartTimeRange = monthsAgo(0);
					break;
	
				case 'lastMonth':
					$this.datetimeRangePicker.searchStartTimeRange = monthsAgo(1);
					break;

				case '1hour':
					const begin =  Date.now() - (1 * 60 * 60 * 1000);
					const end =  Date.now() + (30 * 60 * 1 * 1000);
					$this.datetimeRangePicker.searchStartTimeRange = [begin , end];
					break;
			}
		},

		downloadExcel(config : any)
		{
			// (範例) config參數 請傳入類似以下這樣的物件
			const configSample : any =
			{
				headers: ['用戶ID', '商戶', '時間'],
				headersWithConfig: [ { text: '文字' , width: 18 } , { text: '文字' , width: 18 }], // 18是預設格子寬度
				rowData: [
					[ 'A1內容' , 'B1內容' , 'C1內容'],
					[ 'A2內容' , 'B2內容' , 'C2內容'],
				],
				fileName: 'export_members_20200101.xlsx'
			};      

			// Had to create a new workbook and then add the header
			const ws: any = XLSX.utils.book_new();

			// 設置寬度預設值
			if(config.headersWithConfig == null || config.headersWithConfig.length <= 0)
			{
				config.headersWithConfig = [];
				config.headers.forEach((headerText : string)=>
				{
					config.headersWithConfig.push({text: headerText , width: 18 });
				});
			}

			const myHeaders = config.headersWithConfig.map((x : any)=>  
			{
				return x.text;
			});

			const headers : any = [ myHeaders ];
			XLSX.utils.sheet_add_aoa(ws, headers);

			// Starting in the second row to avoid overriding and skipping headers
			XLSX.utils.sheet_add_json(ws,config.rowData, { origin: 'A2', skipHeader: true });

			const columnWidthSetting: any = [];

			// 欄位預設寬度
			config.headersWithConfig.forEach((item: any) => {
				columnWidthSetting.push({ width: item.width ?? 18 });
			});

			const defaultHeaderStyle = {
				font: {
					bold: true,
				},				
				fill: {
					patternType: "solid",
					fgColor: { rgb: "fff1c7" }
				},
				border: { 
					top: { style: 'thin' , color: {  rgb: "CCCCCC" } },
					bottom: { style: 'thin' , color: {  rgb: "CCCCCC" } },
					left: { style: 'thin'  , color: {  rgb: "CCCCCC" }},
					right: { style: 'thin'  , color: {  rgb: "CCCCCC" }}
				}
			};

			try{
				ws["A1"].s = defaultHeaderStyle;
				ws["B1"].s = defaultHeaderStyle;
				ws["C1"].s = defaultHeaderStyle;
				ws["D1"].s = defaultHeaderStyle;
				ws["E1"].s = defaultHeaderStyle;
				ws["F1"].s = defaultHeaderStyle;
				ws["G1"].s = defaultHeaderStyle;
				ws["H1"].s = defaultHeaderStyle;
				ws["I1"].s = defaultHeaderStyle;
				ws["J1"].s = defaultHeaderStyle;
				ws["K1"].s = defaultHeaderStyle;
				ws["L1"].s = defaultHeaderStyle;
				ws["M1"].s = defaultHeaderStyle;
				ws["N1"].s = defaultHeaderStyle;
				ws["O1"].s = defaultHeaderStyle;
			}
			catch(e)
			{

			}		

			ws['!cols'] = columnWidthSetting;		

			// A workbook is the name given to an Excel file
			const wb = XLSX.utils.book_new();

			XLSX.utils.book_append_sheet(wb, ws, 'Sheet1'); // sheetAName is name of Worksheet

			// export Excel file
			XLSX.writeFile(wb, config.fileName); // name of the file is 'book.xlsx'		

		},


		downloadExcelAndSetEachCellStyle(config : any)
		{
			// (範例) config參數 請傳入類似以下這樣的物件
			const configSample : any =
			{
				rowData: [
					[ { text: 'Row_1001' , style: 1 } , { text: 'Row_1002' , style: 1 } ],
					[ { text: 'Row_2001' , style: 1 } , { text: 'Row_2002' , style: 1 } ],
				],
				fileName: 'export_members_20200101.xlsx'
			};      

			// Had to create a new workbook and then add the header
			const ws: any = XLSX.utils.book_new();

			console.log(config.rowData);

			const eachRowText = [];

			for(const items of config.rowData)
			{
				const myCells: string[] = [];
				for(const item of  items)
				{
					myCells.push(item.text);
				}	
				eachRowText.push(myCells);
			}

			console.log(eachRowText);

			// Starting in the second row to avoid overriding and skipping headers
			XLSX.utils.sheet_add_json(ws, eachRowText, { origin: 'A1', skipHeader: true });
		
			const defaultHeaderStyle = {							
				alignment :{
					horizontal: 'left'
				},
				border: { 
					top: { style: 'thin' , color: {  rgb: "CCCCCC" } },
					bottom: { style: 'thin' , color: {  rgb: "CCCCCC" } },
					left: { style: 'thin'  , color: {  rgb: "CCCCCC" }},
					right: { style: 'thin'  , color: {  rgb: "CCCCCC" }}
				}
			};

			const ligthenyellowBGStyle = {
				font: {
					bold: true,
				},				
				alignment :{
					horizontal: 'left'
				},
				fill: {
					patternType: "solid",
					fgColor: { rgb: "fff1c7" }
				},
				border: { 
					top: { style: 'thin' , color: {  rgb: "CCCCCC" } },
					bottom: { style: 'thin' , color: {  rgb: "CCCCCC" } },
					left: { style: 'thin'  , color: {  rgb: "CCCCCC" }},
					right: { style: 'thin'  , color: {  rgb: "CCCCCC" }}
				}
			};
			const ligthenPurpleBGStyle = {
				font: {
					bold: true,
				},				
				alignment :{
					horizontal: 'left'
				},
				fill: {
					patternType: "solid",
					fgColor: { rgb: "d7b8ff" } 
				},
				border: { 
					top: { style: 'thin' , color: {  rgb: "CCCCCC" } },
					bottom: { style: 'thin' , color: {  rgb: "CCCCCC" } },
					left: { style: 'thin'  , color: {  rgb: "CCCCCC" }},
					right: { style: 'thin'  , color: {  rgb: "CCCCCC" }}
				}
			};


			try{
				let rowNumber = 1;

				const mappingTable = [ 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I' , 'J', 'K', 'L', 'M' , 'N', 'O', 'P', 'Q', 'R' ];

				// @ts-ignore
				for(const items of  config.rowData)
				{		
					let columnNumber = 1;
					for(const item of items)
					{
						const targetCellCode = mappingTable[columnNumber - 1] + rowNumber.toString();

						if(item.style === 1) {
							ws[targetCellCode].s = ligthenyellowBGStyle;
						}
						else if(item.style === 2) {
							ws[targetCellCode].s = ligthenPurpleBGStyle;
						}						
						else {
							ws[targetCellCode].s = defaultHeaderStyle;
						}

						columnNumber++;
					}	

					rowNumber++;
				}				
			}
			catch(e)
			{

			}		


			// 這邊可能要調整成可以動態配置
			ws['!cols'] = [{ width: 35 }, { width: 35 }, { width: 35 } ,{ width: 35 } , { width: 35 }  ];

			// A workbook is the name given to an Excel file
			const wb = XLSX.utils.book_new();

			XLSX.utils.book_append_sheet(wb, ws, 'Sheet1'); // sheetAName is name of Worksheet

			// export Excel file
			XLSX.writeFile(wb, config.fileName); // name of the file is 'book.xlsx'		

		},


		isEnableSimplifyDropdown()
		{			
			const myValue = localStorage.getItem('isEnableSimplifyDropdown');
			return myValue === '1';
		},


		/* 
			用法:  this.formatColumn(ws , 8 , '0%');  
			col代表第幾欄 A=0 B=1 
		*/
		formatColumn(worksheet: any, col: any, fmt: any) {
			const range = XLSX.utils.decode_range(worksheet['!ref']);
			
			// note: range.s.r + 1 skips the header row
			for (let row = range.s.r + 1; row <= range.e.r; ++row) 
			{
			  const ref = XLSX.utils.encode_cell({ r: row, c: col });
			  if (worksheet[ref] && worksheet[ref].t === 'n') {
				worksheet[ref].z = fmt;
			  }
			}		  
		},
		

		copyTextToClipboard(text : string) 
		{
			const textarea = document.createElement('textarea');
			textarea.style.cssText = 'width:1px;height:1px;position: fixed;top: -50px;';

			document.body.appendChild(textarea);

			textarea.value = text;
			textarea.select();
			textarea.focus();

			textarea.setSelectionRange(0, 99999);

			EventBus.$emit(`successfully-copy`);
			console.log('copy success', document.execCommand('copy'));

			document.body.removeChild(textarea);

		},


		/* 這個版本是會換行的，但是可以解決某些modal彈出無法複製的問題 */
		copyTextToClipboard2(text : string) 
		{
			const textarea = document.createElement('textarea');
			textarea.style.cssText = 'width:1px;height:1px;position: fixed;top: -50px;';

			const selection : any = document.getSelection();
		  
			textarea.textContent = text;

			
			const parent : any = document.body;
			parent.appendChild(textarea);

			const range = document.createRange();
			range.selectNode(textarea);
			selection.removeAllRanges();
			selection.addRange(range);

			
			EventBus.$emit(`successfully-copy`);
			console.log('copy success', document.execCommand('copy'));

		},

		getBrandingName()
		{
			const doamin = window.location.hostname;

			if(doamin.includes('localhost') || 
			   doamin.includes('portal.slotsbb'))
			{
				return 'Slot88';
			}

			if(this.isBWGSiteUrl())
			{
				return 'BWG BO';
			}

			return 'BO System (数据后台)';
		},


		isBWGSiteUrl() {
			const doamin = window.location.hostname;
	
			if (doamin.includes('target')) {
				return true;
			}
	
			return false;
		},

		// 偷懶的通用型簡易update功能
		saveSimpleUpdate()
		{
			const vm : any = this;
			const form = vm.simpleUpdateForm;

			EventBus.$emit('show_mask', null);
			SimpleUpdateApi.simpleUpdate(form).then((res)=>
			{	
				EventBus.$emit(`successfully-modified`);
			})
			.finally(()=>
			{
				EventBus.$emit('hide_mask', null);
			});


			try{
				if(form.callback)
				{
					form.callback();
				}
			}catch(e)
			{

			}
			
			
		}



	}


};
