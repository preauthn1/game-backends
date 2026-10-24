<template>
	<v-card>

		<v-card-title v-if="!isHideCloseButton">
			<v-spacer></v-spacer>
			<div class="close" @click="close">
				<v-icon>close</v-icon>
			</div>
		</v-card-title>

		<v-card-text>
			<v-flex id="parent">
				<canvas id="canvas"></canvas>
			</v-flex>
		</v-card-text>
	</v-card>

</template>

<script lang="ts">
import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import * as ChartModel from '@/models/interfaces/statistics/chart';

@Component
// https://example.invalid/jordanwillis/pen/ZeaLGj
export default class LineChartPopup extends Vue {
	dialog: boolean = true;
	@Prop(String) readonly title!: string;
	@Prop(Array) readonly datasets!: any[];
	@Prop(String) readonly xTitle!: string;
	@Prop(String) readonly yTitle!: string;
	@Prop(Object) readonly chartData!: ChartModel.ICommonChartReponse;

	Chart: any;

	isHideCloseButton: boolean = true;
	CanvasHeight: number = 125;
	ticksSuffixText: string = '';
	forceColor: string = '';

	initialCanvasData(myConfig: any = null) {
		console.log(myConfig);
		if (myConfig != null) {
			if (myConfig.isHideCloseButton) {
				this.isHideCloseButton = true;
			}
			if (myConfig.ticksSuffixText != null && myConfig.ticksSuffixText.length > 0) {
				this.ticksSuffixText = myConfig.ticksSuffixText;
			}
			if (myConfig.forceColor != null && myConfig.forceColor.length > 0) {
				this.forceColor = myConfig.forceColor;
			}
			if (myConfig.CanvasHeight && myConfig.CanvasHeight > 0) {
				this.CanvasHeight = myConfig.CanvasHeight;
			}
		} else {
			this.isHideCloseButton = false;
		}

		this.destroyAndCreateNew();

		const Chart = require('chart.js');
		const canvas = <HTMLCanvasElement>document.getElementById('canvas');

		const context: any = canvas.getContext('2d');

		context.save();
		context.setTransform(1, 0, 0, 1, 0, 0);
		context.clearRect(0, 0, canvas.width, canvas.height);
		context.restore();

		canvas.height = this.CanvasHeight;
		const ctx = canvas.getContext('2d');
		let i = 0;
		const color = [23, 100, 203, 360, 285, 43, 286, 15, 99, 205, 132, 314, 17, 50, 2];

		const myChart = new Chart(ctx, {
			type: 'line',
			data: {
				labels: this.chartData.labels,
				datasets: this.chartData.datasets.map((item) => {
					let cssHSL = `hsla(${i < color.length ? color[i] : Math.random() * 360}, 95%, 65%, 1)`;
					i++;

					if (this.forceColor != null && this.forceColor.length > 0) {
						cssHSL = this.forceColor;
					}

					return {
						label: item.groupName,
						data: item.data.map((s) => {
							return {
								x: s.labelName,
								y: s.value,
							};
						}),
						fill: false,
						pointRadius: 2,
						hoverBorderWidth: 8,
						borderWidth: 1.5,
						backgroundColor: cssHSL,
						borderColor: cssHSL,
						lineTension: 0.1,
					};
				}),
			},
			options: {
				responsive: true,
				title: {
					display: true,
					text: this.chartData.chartName,
					fontSize: 15,
					fontFamily: '微軟正黑體, arial',
					fontColor: '#FFFFFF',
				},
				legend: {
					position: 'bottom',
					labels: {
						fontSize: 12,
						boxWidth: 12,
						usePointStyle: true,
						fontFamily: '微軟正黑體, arial',
						fontColor: '#FFFFFF',
					},
				},
				scales: {
					xAxes: [
						{
							gridLines: {
								display: false,
							},
							ticks: {
								fontColor: '#CCC', // this here
							},
						},
					],
					yAxes: [
						{
							gridLines: {
								display: false,
							},
							ticks: {
								fontColor: '#CCC', // this here
								callback: (v: any) => `${v}${this.ticksSuffixText}`,
							},
						},
					],
				},
				tooltips: {
					mode: 'index',
					intersect: false,
				},
				hover: {
					mode: 'nearest',
					intersect: true,
				},
				animation: {
					duration: 1500,
				},
			},
		});

		this.Chart = myChart;
	}

	destroyAndCreateNew() {
		try {
			const canvas = <HTMLCanvasElement>document.getElementById('canvas');
			canvas.remove();

			const parent = <HTMLCanvasElement>document.getElementById('parent');

			const newCanvas = document.createElement('canvas');
			newCanvas.setAttribute('id', 'canvas');
			parent.appendChild(newCanvas);
		} catch (e) {}
	}

	close() {
		this.destroyAndCreateNew();
		this.$emit('close');
	}

	mounted() {}
}
</script>
<style lang="scss" scoped>
.close {
	transition: all 300ms;
}
.close:hover {
	transform: rotate(360deg);
}
</style>
