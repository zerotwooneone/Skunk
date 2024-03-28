/*
select 
	"Type",
	max("UtcMsTimeStamp") as maxtimestamp
FROM "SensorValues"
GROUP BY "SensorValues"."Type"
ORDER BY MaxTimestamp
*/

select 
	"SensorValues".*
FROM "SensorValues"
JOIN (
	select 
		"Type",
		max("UtcMsTimeStamp") as maxtimestamp
	FROM "SensorValues"
	GROUP BY "SensorValues"."Type"
) as maxtype
ON 
	"SensorValues"."Type"=maxtype."Type" 
	AND "SensorValues"."UtcMsTimeStamp"=maxtype."maxtimestamp"
ORDER BY "SensorValues"."UtcMsTimeStamp", "SensorValues"."Id"