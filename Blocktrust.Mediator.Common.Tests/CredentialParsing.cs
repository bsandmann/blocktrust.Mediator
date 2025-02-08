namespace Blocktrust.Mediator.Common.Tests;

using Models.Credential;
using Models.ProblemReport;

public class CredentialParsingTests
{
    [Fact]
    public void LegacyParser_should_pass()
    {
        var workflow_jwt ="eyJhbGciOiJFUzI1NksiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJkaWQ6cHJpc206YzBkNDc4NWM1NjQxODM5MzNhM2FiNTk2YzE1NWQwNWMyYmNmZGI4ZGYxNDA1NGQ1NTczZTFhZTE5YWIxMzE0NTpDcXdEQ3FrREVsb0tCV3RsZVMweEVBUkNUd29KYzJWamNESTFObXN4RWlDMEx3eW5kOWNoeVR4UTg2enhaOUd3RHduNzZ3Yk8wM3lDV0h1SzhwUDZrQm9ndWI5c19rNnY0cGxpMVFqa2ZKb05WdG5CMExmVXNSMGJIb1F1ZzItbUQwNFNXZ29GYTJWNUxUSVFBMEpQQ2dselpXTndNalUyYXpFU0lNd3d2QTc5MjJSdVBTXzlFUTBMOXViWDhSbXFiZG12NTFTNUYweDVaNmtDR2lBS2VUdkZfRWUwV1c5cjhMN2QwRFhjSXVMYk9mdHZ5cGZzdTB3bkRDZm0weEphQ2dWclpYa3RNeEFDUWs4S0NYTmxZM0F5TlRack1SSWd2SHhXbEZHUTVLTkg2ZTVmaHJ2dS1KRjJNWkdPSHlfSlVpdVBWV0V6b0dVYUlMcE9wVWVla28yT3pabWF0UTJZZDFGeGFNUVh6eS00WnB2b0FGQ3BORUNJRWx3S0IyMWhjM1JsY2pBUUFVSlBDZ2x6WldOd01qVTJhekVTSU16blFUd0JkcTUwNmk1ZEJvZ3dNbW5Kc2ZHUEpzcDdlRVY5TldwVHo4UEdHaUMtUnVRNVlPdVQ1MmJrVWVKWXpUcEFJdEljUlJSczFXT3kwTDkyTWpUYjlobzFDZ2x6WlhKMmFXTmxMVEVTRFV4cGJtdGxaRVJ2YldGcGJuTWFHVnNpYUhSMGNITTZMeTl6YjIxbExuTmxjblpwWTJVdklsMCIsInN1YiI6ImRpZDpleGFtcGxlOjEyMyIsIm5iZiI6MTczOTAxNTYzNCwiZXhwIjoxODk2NzgyMDM0LCJ2YyI6eyJAY29udGV4dCI6WyJodHRwczovL3d3dy53My5vcmcvMjAxOC9jcmVkZW50aWFscy92MSJdLCJ0eXBlIjpbIlZlcmlmaWFibGVDcmVkZW50aWFsIl0sImlzc3VlciI6ImRpZDpwcmlzbTpjMGQ0Nzg1YzU2NDE4MzkzM2EzYWI1OTZjMTU1ZDA1YzJiY2ZkYjhkZjE0MDU0ZDU1NzNlMWFlMTlhYjEzMTQ1OkNxd0RDcWtERWxvS0JXdGxlUzB4RUFSQ1R3b0pjMlZqY0RJMU5tc3hFaUMwTHd5bmQ5Y2h5VHhRODZ6eFo5R3dEd243NndiTzAzeUNXSHVLOHBQNmtCb2d1YjlzX2s2djRwbGkxUWprZkpvTlZ0bkIwTGZVc1IwYkhvUXVnMi1tRDA0U1dnb0ZhMlY1TFRJUUEwSlBDZ2x6WldOd01qVTJhekVTSU13d3ZBNzkyMlJ1UFNfOUVRMEw5dWJYOFJtcWJkbXY1MVM1RjB4NVo2a0NHaUFLZVR2Rl9FZTBXVzlyOEw3ZDBEWGNJdUxiT2Z0dnlwZnN1MHduRENmbTB4SmFDZ1ZyWlhrdE14QUNRazhLQ1hObFkzQXlOVFpyTVJJZ3ZIeFdsRkdRNUtOSDZlNWZocnZ1LUpGMk1aR09IeV9KVWl1UFZXRXpvR1VhSUxwT3BVZWVrbzJPelptYXRRMllkMUZ4YU1RWHp5LTRacHZvQUZDcE5FQ0lFbHdLQjIxaGMzUmxjakFRQVVKUENnbHpaV053TWpVMmF6RVNJTXpuUVR3QmRxNTA2aTVkQm9nd01tbkpzZkdQSnNwN2VFVjlOV3BUejhQR0dpQy1SdVE1WU91VDUyYmtVZUpZelRwQUl0SWNSUlJzMVdPeTBMOTJNalRiOWhvMUNnbHpaWEoyYVdObExURVNEVXhwYm10bFpFUnZiV0ZwYm5NYUdWc2lhSFIwY0hNNkx5OXpiMjFsTG5ObGNuWnBZMlV2SWwwIiwidmFsaWRGcm9tIjoiMjAyNS0wMi0wOFQxMTo1Mzo0Ny45ODgzODgiLCJjcmVkZW50aWFsU3ViamVjdCI6eyJpZCI6ImRpZDpleGFtcGxlOjEyMyIsImNsYWltMSI6InRlc3QiLCJjbGFpbTIiOiJ0ZXN0MiJ9fX0.y_pFbnAvO5ob_KjGdY7RihRqFGtQodtG2K8pLw9z7WY6hIQwMuULwzag976zsNVdTI7xjZYx6KkIPeYXTGQE6A";
        var cred = new Credential().Parse(workflow_jwt);
        Assert.NotNull(cred);
    }

}
