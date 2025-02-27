namespace SurrealDB.Core.Tests;

public class ConversionTests {

    public static IEnumerable<object[]> SingleTests {
        get {
            var floatsToTest = new List<float> {
                1,
                0,
                -1,
                float.MaxValue,
                float.MinValue,
                float.PositiveInfinity,
                float.NegativeInfinity,
                float.NaN
            };

            foreach (var floatToTest in floatsToTest) {
                yield return new object[] { floatToTest, floatToTest };
                yield return new object[] { floatToTest.ToString(), floatToTest };

                if (floatToTest == float.PositiveInfinity) {
                    yield return new object[] { NumberFormatInfo.CurrentInfo.PositiveInfinitySymbol, floatToTest }; // The normal ToString() is ∞
                }
                else if (floatToTest == float.NegativeInfinity) {
                    yield return new object[] { NumberFormatInfo.CurrentInfo.NegativeInfinitySymbol, floatToTest }; // The normal ToString() is -∞
                }
            }

            yield return new object[] { null!, null! };
        }
    }

    [Theory]
    [MemberData(nameof(SingleTests))]
    public void FloatConverterTests(object testValue, float expectedValue) {
        var json = JsonSerializer.Serialize(testValue, SerializerOptions.Shared);
        var result = JsonSerializer.Deserialize<float>(json, SerializerOptions.Shared);
        result.Should().Be(expectedValue);
    }

    public static IEnumerable<object[]> DoubleTests {
        get {
            var doublesToTest = new List<double> {
                1,
                0,
                -1,
                double.MaxValue,
                double.MinValue,
                double.PositiveInfinity,
                double.NegativeInfinity,
                double.NaN
            };

            foreach (var doubleToTest in doublesToTest) {
                yield return new object[] { doubleToTest, doubleToTest };
                yield return new object[] { doubleToTest.ToString(), doubleToTest };

                if (doubleToTest == float.PositiveInfinity) {
                    yield return new object[] { NumberFormatInfo.CurrentInfo.PositiveInfinitySymbol, doubleToTest }; // The normal ToString() is ∞
                }
                else if (doubleToTest == float.NegativeInfinity) {
                    yield return new object[] { NumberFormatInfo.CurrentInfo.NegativeInfinitySymbol, doubleToTest }; // The normal ToString() is -∞
                }
            }

            yield return new object[] { null!, null! };
        }
    }

    [Theory]
    [MemberData(nameof(DoubleTests))]
    public void DoubleConverterTests(object testValue, double expectedValue) {
        var json = JsonSerializer.Serialize(testValue, SerializerOptions.Shared);
        var result = JsonSerializer.Deserialize<double>(json, SerializerOptions.Shared);
        result.Should().Be(expectedValue);
    }

    public static IEnumerable<object[]> DecimalTests {
        get {
            var decimalsToTest = new List<decimal> {
                1,
                0,
                -1,
                decimal.MaxValue,
                decimal.MinValue
            };

            foreach (var decimalToTest in decimalsToTest) {
                yield return new object[] { decimalToTest, decimalToTest };
                yield return new object[] { decimalToTest.ToString(), decimalToTest };
            }

            yield return new object[] { null!, null! };
        }
    }

    [Theory]
    [MemberData(nameof(DecimalTests))]
    public void DecimalConverterTests(object testValue, decimal expectedValue) {
        var json = JsonSerializer.Serialize(testValue, SerializerOptions.Shared);
        var result = JsonSerializer.Deserialize<decimal>(json, SerializerOptions.Shared);
        result.Should().Be(expectedValue);
    }

    public static IEnumerable<object[]> TimeSpanTests {
        get {
            var timeSpansToTest = new List<TimeSpan> {
                new TimeSpan(1, 2, 3, 4, 5),
                new TimeSpan(200, 20, 34, 41, 65),
                TimeSpan.Zero,
                // Min and Max contain Ticks, the serializer ignores everything below ms when serializing, deserialization with us prec is supported!
                // TimeSpan.MaxValue, 
                // TimeSpan.MinValue,
            };

            foreach (var timeSpanToTest in timeSpansToTest) {
                yield return new object[] { timeSpanToTest, timeSpanToTest };

                yield return new object[] { timeSpanToTest.ToString("c"), timeSpanToTest };

                yield return new object[] { timeSpanToTest.Ticks, timeSpanToTest };
            }

            // .NET TimeSpans only work up to a resolution of 100ns (0.0000001 seconds).
            // But surreal can work with a resolution up to 1ns (0.000000001 seconds).
            // So make sure we can handle these dates with a loss in resolution where times less than 100ns is truncated
            // Test that we don't throw exceptions when parsing into the 10 picoseconds range just in case we are asked to parse something that isn't actually a Surreal Date Time. 
            var ts = new TimeSpan(5, 57, 32);
            yield return new object[] { "05:57:32Z", ts };
            yield return new object[] { "05:57:32.0000000Z", ts };
            yield return new object[] { "05:57:32.000000000Z", ts };
            yield return new object[] { "05:57:32.00000000000Z", ts };
            
            ts = ts.Add(TimeSpan.FromTicks(3294704));
            yield return new object[] { "05:57:32.3294704Z", ts };
            yield return new object[] { "05:57:32.329470400Z", ts };
            yield return new object[] { "05:57:32.32947040000Z", ts };

            
            yield return new object[] { "05:57:32.329470455Z", ts };
            yield return new object[] { "05:57:32.32947045566Z", ts };

            yield return new object[] { null!, null! };
        }
    }

    [Theory]
    [MemberData(nameof(TimeSpanTests))]
    public void TimeSpanConverterTests(object testValue, TimeSpan expectedValue) {
        var json = JsonSerializer.Serialize(testValue, SerializerOptions.Shared);
        var result = JsonSerializer.Deserialize<TimeSpan>(json, SerializerOptions.Shared);
        result.Should().Be(expectedValue);
    }

    public static IEnumerable<object[]> DateTimeOffsetTests {
        get {
            var dateTimesOffsetToTest = new List<DateTimeOffset> {
                new DateTimeOffset(2012, 6, 12, 10, 5, 32, 648, TimeSpan.Zero),
                DateTimeOffset.MaxValue.ToUniversalTime(),
                DateTimeOffset.MinValue.ToUniversalTime(),
            };

            foreach (var dateTimeOffsetToTest in dateTimesOffsetToTest) {
                yield return new object[] { dateTimeOffsetToTest, dateTimeOffsetToTest };

                yield return new object[] { dateTimeOffsetToTest.ToString("O", CultureInfo.InvariantCulture), dateTimeOffsetToTest };
                yield return new object[] { dateTimeOffsetToTest.ToString("u", CultureInfo.InvariantCulture), dateTimeOffsetToTest.AddTicks(-dateTimeOffsetToTest.Ticks % TimeSpan.TicksPerSecond) };
                yield return new object[] { dateTimeOffsetToTest.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture), dateTimeOffsetToTest.AddTicks(-dateTimeOffsetToTest.Ticks % TimeSpan.TicksPerMillisecond) };
                yield return new object[] { dateTimeOffsetToTest.ToString("yyyy/MM/ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture), dateTimeOffsetToTest.AddTicks(-dateTimeOffsetToTest.Ticks % TimeSpan.TicksPerMillisecond) };

                yield return new object[] { dateTimeOffsetToTest.ToUnixTimeSeconds(), dateTimeOffsetToTest.AddTicks(-dateTimeOffsetToTest.Ticks % TimeSpan.TicksPerSecond) };
            }

            // .NET DateTimeOffsets only work up to a resolution of 100ns (0.0000001 seconds).
            // But surreal can work with a resolution up to 1ns (0.000000001 seconds).
            // So make sure we can handle these dates with a loss in resolution where times less than 100ns is truncated
            // Test that we don't throw exceptions when parsing into the 10 picoseconds range just in case we are asked to parse something that isn't actually a Surreal Date Time. 
            var dto = new DateTimeOffset(2022, 10, 16, 5, 57, 32, TimeSpan.Zero);
            yield return new object[] { "2022-10-16T05:57:32Z", dto };
            yield return new object[] { "2022-10-16T05:57:32.0000000Z", dto };
            yield return new object[] { "2022-10-16T05:57:32.000000000Z", dto };
            yield return new object[] { "2022-10-16T05:57:32.00000000000Z", dto };

            yield return new object[] { "2022-10-16T05:57:32+00:00", dto };
            yield return new object[] { "2022-10-16T05:57:32.0000000+00:00", dto };
            yield return new object[] { "2022-10-16T05:57:32.000000000+00:00", dto };
            yield return new object[] { "2022-10-16T05:57:32.00000000000+00:00", dto };
            
            dto = dto.AddTicks(3294704);
            yield return new object[] { "2022-10-16T05:57:32.3294704Z", dto };
            yield return new object[] { "2022-10-16T05:57:32.329470400Z", dto };
            yield return new object[] { "2022-10-16T05:57:32.32947040000Z", dto };

            yield return new object[] { "2022-10-16T05:57:32.3294704+00:00", dto };
            yield return new object[] { "2022-10-16T05:57:32.329470400+00:00", dto };
            yield return new object[] { "2022-10-16T05:57:32.32947040000+00:00", dto };
            
            yield return new object[] { "2022-10-16T05:57:32.329470455Z", dto };
            yield return new object[] { "2022-10-16T05:57:32.32947045566Z", dto };
            
            yield return new object[] { "2022-10-16T05:57:32.329470455+00:00", dto };
            yield return new object[] { "2022-10-16T05:57:32.32947045566+00:00", dto };

            yield return new object[] { null!, null! };
        }
    }

    [Theory]
    [MemberData(nameof(DateTimeOffsetTests))]
    public void DateTimeOffsetConverterTests(object testValue, DateTimeOffset expectedValue) {
        var json = JsonSerializer.Serialize(testValue, SerializerOptions.Shared);
        var result = JsonSerializer.Deserialize<DateTimeOffset>(json, SerializerOptions.Shared);
        result.Should().Be(expectedValue);
    }

    public static IEnumerable<object[]> DateTimeTests {
        get {
            var dateTimesToTest = new List<DateTime> {
                new DateTime(2012, 6, 12, 10, 5, 32, 648, DateTimeKind.Utc),
                new DateTime(2012, 10, 2, 20, 55, 54, 3, DateTimeKind.Utc),
                new DateTime(2012, 12, 2, 1, 2, 3, 4, DateTimeKind.Utc),
                DateTime.MaxValue.AsUtc(),
                DateTime.MinValue.AsUtc(),
            };

            foreach (var dateTimeToTest in dateTimesToTest) {
                yield return new object[] { dateTimeToTest, dateTimeToTest };
                yield return new object[] { dateTimeToTest.Date, dateTimeToTest.Date };

                yield return new object[] { dateTimeToTest.ToString("O", CultureInfo.InvariantCulture), dateTimeToTest };
                yield return new object[] { dateTimeToTest.Date.ToString("O", CultureInfo.InvariantCulture), dateTimeToTest.Date };
                yield return new object[] { dateTimeToTest.ToString("u", CultureInfo.InvariantCulture), dateTimeToTest.AddTicks(-dateTimeToTest.Ticks % TimeSpan.TicksPerSecond) };
                yield return new object[] { dateTimeToTest.Date.ToString("u", CultureInfo.InvariantCulture), dateTimeToTest.Date };
                yield return new object[] { dateTimeToTest.ToString("yyyy-MM-ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture), dateTimeToTest.AddTicks(-dateTimeToTest.Ticks % TimeSpan.TicksPerMillisecond) };
                yield return new object[] { dateTimeToTest.ToString("yyyy/MM/ddTHH:mm:ss.fffK", CultureInfo.InvariantCulture), dateTimeToTest.AddTicks(-dateTimeToTest.Ticks % TimeSpan.TicksPerMillisecond) };

                yield return new object[] { new DateTimeOffset(dateTimeToTest).ToUnixTimeSeconds(), dateTimeToTest.AddTicks(-dateTimeToTest.Ticks % TimeSpan.TicksPerSecond) };
            }

            // .NET DateTimes only work up to a resolution of 100ns (0.0000001 seconds).
            // But surreal can work with a resolution up to 1ns (0.000000001 seconds).
            // So make sure we can handle these dates with a loss in resolution where times less than 100ns is truncated
            // Test that we don't throw exceptions when parsing into the 10 picoseconds range just in case we are asked to parse something that isn't actually a Surreal Date Time. 
            var dt = new DateTime(2022, 10, 16, 5, 57, 32, DateTimeKind.Utc);
            yield return new object[] { "2022-10-16T05:57:32Z", dt };
            yield return new object[] { "2022-10-16T05:57:32.0000000Z", dt };
            yield return new object[] { "2022-10-16T05:57:32.000000000Z", dt };
            yield return new object[] { "2022-10-16T05:57:32.00000000000Z", dt };
            
            dt = dt.AddTicks(3294704);
            yield return new object[] { "2022-10-16T05:57:32.3294704Z", dt };
            yield return new object[] { "2022-10-16T05:57:32.329470400Z", dt };
            yield return new object[] { "2022-10-16T05:57:32.32947040000Z", dt };

            
            yield return new object[] { "2022-10-16T05:57:32.329470455Z", dt };
            yield return new object[] { "2022-10-16T05:57:32.32947045566Z", dt };


            yield return new object[] { null!, null! };
        }
    }

    [Theory]
    [MemberData(nameof(DateTimeTests))]
    public void DateTimeConverterTests(object testValue, DateTime expectedValue) {
        var json = JsonSerializer.Serialize(testValue, SerializerOptions.Shared);
        var result = JsonSerializer.Deserialize<DateTime>(json, SerializerOptions.Shared);
        result.Should().Be(expectedValue);
    }

    public static IEnumerable<object[]> DateTests {
        get {
            var datesToTest = new List<DateOnly> {
                new DateOnly(2012, 6, 12),
                new DateOnly(2012, 10, 2),
                DateOnly.MaxValue,
                DateOnly.MinValue,
            };

            foreach (var dateToTest in datesToTest) {
                yield return new object[] { dateToTest, dateToTest};

                yield return new object[] { dateToTest.ToString("O", CultureInfo.InvariantCulture), dateToTest };
                yield return new object[] { dateToTest.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), dateToTest };
                yield return new object[] { dateToTest.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture), dateToTest };
            }

            yield return new object[] { null!, null! };
        }
    }

    [Theory]
    [MemberData(nameof(DateTests))]
    public void DateConverterTests(object testValue, DateOnly expectedValue) {
        var json = JsonSerializer.Serialize(testValue, SerializerOptions.Shared);
        var result = JsonSerializer.Deserialize<DateOnly>(json, SerializerOptions.Shared);
        result.Should().Be(expectedValue);
    }

    public static IEnumerable<object[]> TimeTests {
        get {
            var timesToTest = new List<TimeOnly> {
                new TimeOnly(10, 5, 32, 648),
                new TimeOnly(20, 55, 54, 3),
                new TimeOnly(1, 2, 3, 4),
                TimeOnly.MaxValue,
                TimeOnly.MinValue,
            };

            foreach (var timeToTest in timesToTest) {
                yield return new object[] { timeToTest, timeToTest };

                yield return new object[] { timeToTest.ToString("O", CultureInfo.InvariantCulture), timeToTest };
                yield return new object[] { timeToTest.ToString("R", CultureInfo.InvariantCulture), timeToTest.Add(new TimeSpan(-timeToTest.Ticks % TimeSpan.TicksPerSecond)) };
                yield return new object[] { timeToTest.ToString("T", CultureInfo.InvariantCulture), timeToTest.Add(new TimeSpan(-timeToTest.Ticks % TimeSpan.TicksPerSecond)) };
                yield return new object[] { timeToTest.ToString("HH:mm:ss.fffK", CultureInfo.InvariantCulture), timeToTest.Add(new TimeSpan(-timeToTest.Ticks % TimeSpan.TicksPerMillisecond)) };
            }

            // .NET Times only work up to a resolution of 100ns (0.0000001 seconds).
            // But surreal can work with a resolution up to 1ns (0.000000001 seconds).
            // So make sure we can handle these dates with a loss in resolution where times less than 100ns is truncated
            // Test that we don't throw exceptions when parsing into the 10 picoseconds range just in case we are asked to parse something that isn't actually a Surreal Date Time. 
            var t = new TimeOnly(5, 57, 32);
            yield return new object[] { "05:57:32Z", t };
            yield return new object[] { "05:57:32.0000000Z", t };
            yield return new object[] { "05:57:32.000000000Z", t };
            yield return new object[] { "05:57:32.00000000000Z", t };
            
            t = t.Add(TimeSpan.FromTicks(3294704));
            yield return new object[] { "05:57:32.3294704Z", t };
            yield return new object[] { "05:57:32.329470400Z", t };
            yield return new object[] { "05:57:32.32947040000Z", t };

            
            yield return new object[] { "05:57:32.329470455Z", t };
            yield return new object[] { "05:57:32.32947045566Z", t };

            yield return new object[] { null!, null! };
        }
    }

    [Theory]
    [MemberData(nameof(TimeTests))]
    public void TimeConverterTests(object testValue, TimeOnly expectedValue) {
        var json = JsonSerializer.Serialize(testValue, SerializerOptions.Shared);
        var result = JsonSerializer.Deserialize<TimeOnly>(json, SerializerOptions.Shared);
        result.Should().Be(expectedValue);
    }
}
