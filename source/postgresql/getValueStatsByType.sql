select
	"Type",
	min("Value") minValue,
	max("Value") maxValue,
	avg("Value") avgValue,
	min("UtcMsTimeStamp") minTimestamp,
	max("UtcMsTimeStamp") maxTimestamp,
	count("Id"),
	stddev("Value") stddev_value,
	stddev_pop("Value") stddevpop_value
FROM "SensorValues"
GROUP BY "Type"