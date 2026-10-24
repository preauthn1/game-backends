import UserProfile from '@/store/auth';


export const formatMixin = {
	methods:  {

		toAmountDisplay(amount: number) {
			if (amount === 0) {
				return 0;
			}
			return amount
				? Number(amount)
						.toFixed(2)
						.toString()
						.replace(/(\d{1,3})(?=(\d{3})+\.)/g, '$1,')
				: // .replace(/\B(?=(\d{3})+(?!\d))/g, ',')
				  '---';
		}		
	},
};




export const datetimeMixin = {
	filters: {
		timeWithAddTimezone(datetime: string, timezone: number = websiteTimezone) {
			return timeWithTimezone(datetime, timezone);
		},
		toHHMMSS(secs: number) {
			const hours = Math.floor(secs / 3600);
			const minutes = Math.floor(secs / 60) % 60;
			const seconds = secs % 60;

			return [hours, minutes, seconds]
				.map(v => (v < 10 ? '0' + v : v))
				.filter((v, i) => v !== '00' || i > 0)
				.join(':');
		},
	},

	methods: {		
		toTimeWithAddTimezone(datetime: string, timezone: number = websiteTimezone) {
			return timeWithTimezone(datetime, timezone);
		}
	}
};

/**
 * 站台timezone
 * for filter timestamp to string
 */
export const websiteTimezone = 8;
export const unknowTimezone = -999;



/*
	透過vuex state取得當前用戶時區
 */
export const getUserProfileTimezone = () => {

	try{
		return UserProfile.state.userProfile.timeZone;
	}catch(e)
	{

	}
	return websiteTimezone;
};


/**
 * 開始日期/結束日期 加上自定義時區
 * @parameter datetime 日期
 * @parameter day 加上的天數
 */
export const startEndDate = (time: number, day: number = 0, timezone: number = unknowTimezone) => {

	if(timezone === unknowTimezone)
	{
		timezone = getUserProfileTimezone();
	}

	// 扣除自定義時區，用UTC+0向server詢問
	const userTimezoneOffset = new Date(time).getTimezoneOffset() * 60000;
	const datetimeWithTimezone = time - userTimezoneOffset - timezone * 60 * 60 * 1000;
	// 加上日期
	const timestamp = new Date(datetimeWithTimezone).getTime() + day * 24 * 60 * 60 * 1000;
	const webTime = new Date(timestamp);
	// Date
	const iso = webTime
		.toISOString()
		.slice(0, 10)
		.replace(/-/g, '-');
	// Time 這裡用UTC取得已經轉換過的時間
	// const time = webTime;
	const hours = webTime.getUTCHours() < 10 ? `0${webTime.getUTCHours()}` : webTime.getUTCHours();
	const minutes = `0${webTime.getUTCMinutes()}`;
	const seconds = `0${webTime.getUTCSeconds()}`;
	return `${iso} ${hours}:${minutes.substr(-2)}:${seconds.substr(-2)}`;
};

// 根據用戶時區 顯示yyyyMMdd
export const toLocalDateString = (time: number, timezone: number| null = null) => {

	// bug案例。凌晨00點多。在韓國(+9時區) 操作的時候回拿到前一日
	if(timezone == null) {	
		timezone = new Date().getTimezoneOffset() / -60;
		// timezone = 8;
	}

	// 扣除自定義時區，用UTC+0向server詢問
	const webTime = new Date(time + timezone * 3600 * 1000);

	// Date
	const iso = webTime
		.toISOString()
		.slice(0, 10)
		.replace(/-/g, '-');

	return `${iso}`;
};


export const toLocalDateStringYYYYMMDD = (time: number, timezone: number = websiteTimezone) => {
	// 扣除自定義時區，用UTC+0向server詢問
	const webTime = new Date(time + timezone * 3600 * 1000);

	let iso : any = webTime
		.toISOString()
		.slice(0, 10)
		.replace(/-/g, '-');
		
	iso = iso.replaceAll('-' , '');
	return `${iso}`;
};



// 根據用戶時區 顯示MMdd (不含年分)
export const toLocalDateMMdd = (time: number, timezone: number = websiteTimezone , seperateText : string = '-') => {
	// 扣除自定義時區，用UTC+0向server詢問
	const webTime = new Date(time + timezone * 3600 * 1000);

	// Date
	const iso = webTime
		.toISOString()
		.slice(5, 10)
		.replace(/-/g, seperateText);

	return `${iso}`;
};



export const toDateStringYYYYMMDDHHMMSS = (time: number, timezone: number = websiteTimezone) => {
	// 扣除自定義時區，用UTC+0向server詢問
	const webTime = new Date(time + timezone * 3600 * 1000);

	// Date
	const iso = webTime
		.toISOString()
		.slice(0, 10)
		.replace(/-/g, '-');
	// Time 這裡用UTC取得已經轉換過的時間
	// const time = webTime;
	const hours = webTime.getUTCHours() < 10 ? `${webTime.getUTCHours()}` : webTime.getUTCHours();
	const minutes = `${webTime.getUTCMinutes()}`;
	const seconds = `${webTime.getUTCSeconds()}`;
	return `${iso}` + ' ' +  myPadLeft(hours.toString()) + ':' + myPadLeft(minutes) + ':' + myPadLeft(seconds);
};

export const toDateStringYYYYMMDDByDateText = (dateText: any , timeZone: number) => {	
	const myDateText = dateText.toString();
	const timestamp = dateStringToTimestamp(myDateText , timeZone);
	
	// @ts-ignore
	return  toLocalDateString(timestamp , timeZone).replaceAll('-' , '').replaceAll('/' , '').substring(0 , 8);
};

export const replaceToNoneIOS8601 = (iso8601: string) => {
	return iso8601.replace('T', ' ').replace('Z', '');
};

export const toDateStringMMDDByDateText = (dateText: any , timeZone: number) => {	
	const myDateText = dateText.toString();
	const timestamp = dateStringToTimestamp(myDateText , timeZone);

	// @ts-ignore
	return  toLocalDateString(timestamp , timeZone).replaceAll('-' , '').replaceAll('/' , '').substring(4 , 8);
};

export const myPadLeft = (timeString: string) => {	
	return timeString.length >= 2 ? timeString : new Array(2 - timeString.length + 1).join("0") + timeString;
};


/**
 * 時間加上自定義時區
 * @parameter datetime 日期
 * @parameter timezone 定義時區
 */
export const timeWithTimezone = (datetime: string, timezone: number = websiteTimezone) => {
	if (!datetime) {
		return ` - `;
	}
	const userTimezoneOffset = 0;
	// 扣除js時區
	const datetimeWithoutTimezone = new Date(datetime).getTime() - userTimezoneOffset;
	let timestamp = new Date(datetimeWithoutTimezone).getTime() / 1000;
	// 加上自定義時區
	timestamp = timestamp * 1000 + timezone * 60 * 60 * 1000;
	const webTime = new Date(timestamp);
	// Date
	const iso = webTime
		.toISOString()
		.slice(0, 10)
		.replace(/-/g, '-');
	// Time 這裡用UTC取得已經轉換過的時間
	// const time = webTime;
	const hours = webTime.getUTCHours() < 10 ? `0${webTime.getUTCHours()}` : webTime.getUTCHours();
	const minutes = `0${webTime.getUTCMinutes()}`;
	const seconds = `0${webTime.getUTCSeconds()}`;
	return `${iso} ${hours}:${minutes.substr(-2)}:${seconds.substr(-2)}`;
};


export const dateStringToTimestamp = (datetime: string, timezone: number = websiteTimezone) => {
	if (!datetime) {
		return 0;
	}	
	const userTimezoneOffset = 0;
	// 扣除js時區
	const datetimeWithoutTimezone = new Date(datetime).getTime() - userTimezoneOffset;
	let timestamp = new Date(datetimeWithoutTimezone).getTime() / 1000;
	// 加上自定義時區
	timestamp = timestamp * 1000 + timezone * 60 * 60 * 1000;
	return timestamp;
};


/**
 * 取得幾天後的日期 yyyy-mm-dd
 * @parameter 傳入7可取得7天後的時間戳
 */
export const daysAgo = (day: number) => {
	const newDate = `${new Date().getFullYear()}-${new Date().getMonth() + 1}-${new Date().getDate()}`;
	const convertDate = new Date(newDate);
	convertDate.setHours(0, 0, 0);

	return new Date(convertDate).getTime() + day * 24 * 60 * 60 * 1000;
};

export const hourAgo = (hour: number) => {
	const newDate = `${new Date().getFullYear()}-${new Date().getMonth() + 1}-${new Date().getDate()}`;
	const convertDate = new Date(newDate);
	convertDate.setHours(0, 0, 0);

	return new Date(convertDate).getTime() + hour * 60 * 60 * 1000;
};


/**
 *
 * @param week 本週:0 ； 上週:1
 */
export const weeksAgo = (week: number) => {
	const now = new Date();
	const nowTime = now.getTime();
	const day = now.getDay();
	const oneDayTime = 24 * 60 * 60 * 1000;

	const mondayTime = nowTime - (day - 1) * oneDayTime;
	const startDate = new Date(mondayTime);
	startDate.setHours(0, 0, 0);

	let sundayTime = nowTime + (7 - day) * oneDayTime;
	sundayTime += oneDayTime;
	const endDate = new Date(sundayTime);
	endDate.setHours(0, 0, 0);

	return [new Date(startDate).getTime() - week * 7 * 24 * 60 * 60 * 1000, new Date(endDate).getTime() - week * 7 * 24 * 60 * 60 * 1000];
};

/**
 *
 * @param month 本月:0 ； 上個月:1
 */
export const monthsAgo = (month: number) => {
	const startDate = new Date();
	

	startDate.setDate(1);
	startDate.setMonth(startDate.getMonth() - month);
	startDate.setHours(0, 0, 0);

	const endDate = new Date();
	const targetMonth = endDate.getMonth() + 1 - month;
	
	endDate.setDate(1);
	endDate.setMonth(targetMonth);
	endDate.setHours(0, 0, 0);
	endDate.setDate(endDate.getDate());

	return [new Date(startDate).getTime(), new Date(endDate).getTime()];
};


export const addMinutes = (targetTime : string , minutes: number , timeZone : number) => {

	const time = new Date(targetTime);

	const ts = time.getTime();

	const offset = minutes * 60 * 1000 + timeZone * 60 * 60 * 1000;

	const newTs = ts + offset;

	const newTime = new Date(newTs);

	const iso = newTime
		.toISOString()
		.slice(0, 10)
		.replace(/-/g, '-');
		
	const hours = newTime.getUTCHours() < 10 ? `0${newTime.getUTCHours()}` : newTime.getUTCHours();
	const myMinutes = `0${newTime.getUTCMinutes()}`;
	const seconds = `0${newTime.getUTCSeconds()}`;
	return `${iso} ${hours}:${myMinutes.substr(-2)}:${seconds.substr(-2)}`;

};
