using System;
using System.Linq;
using System.ServiceModel;
using NUnit.Framework;

namespace Netrika.Test
{
    /// <summary>
    /// Test Suite for AddPatient method.
    /// </summary>
    [TestFixture]
    public class AddPatientTest
    {
        #region Constants

        /// <summary>
        /// Authorization token to get access to methods.
        /// </summary>
        private const string GUID = "8CDE415D-FAB7-4809-AA37-8CDD70B1B46C";

        /// <summary>
        /// Medical organization Id.
        /// </summary>
        private const string IDLPU = "1.2.643.5.1.13.3.25.78.118";

        #endregion

        #region Private fields

        /// <summary>
        /// Netrika API client service.
        /// </summary>
        private PixServiceClient _client;

        #endregion

        #region Init\cleanup methods

        /// <summary>
        /// Opening of client.
        /// </summary>
        [OneTimeSetUp]
        public void Init()
        {
            _client = new PixServiceClient();
            _client.Open();
        }

        /// <summary>
        /// Closing of client.
        /// </summary>
        [OneTimeTearDown]
        public void Dispose()
        {
            _client.Close();
        }

        #endregion

        #region Test cases

        /// <summary>
        /// Checks state of service after opening.
        /// </summary>
        [Test]
        public void OpenServiceClientTest()
        {
            Assert.AreEqual(CommunicationState.Opened, _client.State);
        }

        /// <summary>
        /// Checks adding patient with minimal mandatory data.
        /// </summary>
        [Test]
        public void AddSimplePatientTest()
        {
            // add new patient
            var patient = new PatientDto
            {
                BirthDate = DateTime.Parse("07.06.1928"),
                Sex = 1,
                FamilyName = "Ann",
                GivenName = "Smith",
                IdPatientMIS = Guid.NewGuid().ToString(),
            };

            _client.AddPatient(GUID, IDLPU, patient);

            // get this patient from server
            var queryParameters = new PatientDto { IdPatientMIS = patient.IdPatientMIS };
            var response = _client.GetPatient(GUID, IDLPU, queryParameters, SourceType.Reg);
            var actualPatient = response.Single(p => p.IdPatientMIS == patient.IdPatientMIS);

            // check patient
            Assert.AreEqual(patient.IdPatientMIS, actualPatient.IdPatientMIS);
            Assert.AreEqual(patient.GivenName, actualPatient.GivenName);
            Assert.AreEqual(patient.FamilyName, actualPatient.FamilyName);
            Assert.AreEqual(patient.BirthDate, actualPatient.BirthDate);
        }

        /// <summary>
        /// Checks adding patient with empty GUID (auth token) .
        /// </summary>
        [Test]
        public void EmptyGuidTest()
        {
            Assert.Throws<FaultException<RequestFault>>(() =>
            {
                // add patient with empty GUID
                _client.AddPatient(string.Empty, IDLPU, new PatientDto
                {
                    BirthDate = DateTime.Parse("07.06.1928"),
                    DeathTime = DateTime.Parse("07.05.1920"),
                    Sex = 1,
                    FamilyName = "Ann",
                    GivenName = "Smith",
                    IdPatientMIS = Guid.NewGuid().ToString(),
                });
            });
        }

        /// <summary>
        /// Checks error code of adding patient with empty GUID (auth token).
        /// </summary>
        [Test]
        public void EmptyGuidErrorCodeTest()
        {
            try
            {
                // add patient with empty GUID
                _client.AddPatient(string.Empty, IDLPU, new PatientDto
                {
                    BirthDate = DateTime.Parse("07.06.1928"),
                    DeathTime = DateTime.Parse("07.05.1920"),
                    Sex = 1,
                    FamilyName = "Ann",
                    GivenName = "Smith",
                    IdPatientMIS = Guid.NewGuid().ToString(),
                });
            }
            catch (FaultException<RequestFault> ex)
            {
                // error code 1 = invalid system identification
                Assert.AreEqual(1, ex.Detail.ErrorCode);
            }
            catch
            {
                Assert.Fail("Incorrect exception type.");
            }
        }

        #endregion
    }
}
